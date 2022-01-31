namespace WoWChat.Net.Realm;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Events;
using Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class RealmConnector : IObservable<RealmEvent>
{
  private readonly RealmChannelInitializer _channelInitializer;
  private readonly IEventLoopGroup _group;
  private readonly WowChatOptions _options;
  private readonly ILogger<RealmConnector> _logger;

  protected IDictionary<int, IPacketCommand<RealmEvent>> _packetCommands = new Dictionary<int, IPacketCommand<RealmEvent>>();

  private IChannel? _realmChannel;

  public RealmConnector(
    IServiceProvider serviceProvider,
    RealmChannelInitializer realmChannelInitializer,
    IEventLoopGroup group,
    IOptionsSnapshot<WowChatOptions> options,
    ILogger<RealmConnector> logger
    )
  {
    _channelInitializer = realmChannelInitializer ?? throw new ArgumentNullException(nameof(realmChannelInitializer));
    _group = group ?? throw new ArgumentNullException(nameof(group));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    InitPacketCommands(serviceProvider, _options.GetExpansion());
  }

  public IEventLoopGroup Group { get { return _group; } }

  public RealmPacketHandler RealmPacketHandler { get { return _channelInitializer.RealmPacketHandler; } }

  /// <summary>
  /// Binds the game packet commands defined by the service provider. Called by the constructor
  /// </summary>
  /// <param name="serviceProvider"></param>
  /// <exception cref="InvalidOperationException"></exception>
  private void InitPacketCommands(IServiceProvider serviceProvider, WoWExpansion expansion)
  {
    // Init PacketCommands
    var packetCommandType = typeof(IPacketCommand<RealmEvent>);
    var packetCommandAttributeType = typeof(PacketCommandAttribute);
    var handlerTypes = serviceProvider.GetServices(packetCommandType)
      .Select(o => o?.GetType())
      .Where(p => packetCommandType.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetCommandAttributeType));

    var packetCommandRealmEventCallback = new Action<RealmEvent>((gameEvent) => OnRealmEvent(gameEvent));

    foreach (var handlerType in handlerTypes)
    {
      if (handlerType == default) continue;

      var packetCommandAttributes = handlerType.GetCustomAttributes(false).OfType<PacketCommandAttribute>();

      foreach (var packetCommandAttribute in packetCommandAttributes)
      {
        if ((packetCommandAttribute.Expansion & expansion) != expansion)
        {
          _logger.LogInformation($"Skipping {handlerType} for {expansion} (indicates {packetCommandAttribute.Expansion})");
          continue;
        }

        if (_packetCommands.ContainsKey(packetCommandAttribute.Id))
        {
          throw new InvalidOperationException($"A packet command is already registered for packet id {packetCommandAttribute.Id} ({BitConverter.ToString(packetCommandAttribute.Id.ToBytes())}), expansion: {packetCommandAttribute.Expansion}");
        }

        var packetCommand = (IPacketCommand<RealmEvent>)serviceProvider.GetRequiredService(handlerType);
        packetCommand.CommandId = packetCommandAttribute.Id;
        packetCommand.EventCallback = packetCommandRealmEventCallback;

        _packetCommands.Add(packetCommandAttribute.Id, packetCommand);
      }
    }
  }

  public async Task Connect()
  {
    if (_realmChannel != null && _realmChannel.Active)
    {
      throw new InvalidOperationException($"Refusing to connect to realm server. Already connected.");
    }

    OnRealmEvent(new RealmConnectingEvent()
    {
      Host = _options.RealmListHost,
      Port = _options.RealmListPort,
    });

    var bootstrap = new Bootstrap();
    bootstrap.Group(_group)
      .Channel<TcpSocketChannel>()
      .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_options.ConnectTimeoutMs))
      .Option(ChannelOption.SoKeepalive, true)
      .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
      .RemoteAddress(_options.RealmListHost, _options.RealmListPort)
      .Handler(_channelInitializer);

    try
    {
      var cancelTask = Task.Delay(_options.ConnectTimeoutMs);
      var connectTask = bootstrap.ConnectAsync();

      //double await so if cancelTask throws exception, this throws it
      await await Task.WhenAny(connectTask, cancelTask);

      if (cancelTask.IsCompleted)
      {
        //If cancelTask and connectTask both finish at the same time,
        //we'll consider it to be a timeout.
        throw new TimeoutException();
      }

      _realmChannel = connectTask.Result;
      OnRealmEvent(new RealmConnectedEvent()
      {
        Name = _options.RealmName,
        Host = _options.RealmListHost,
        Port = _options.RealmListPort
      });
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to connect to realm server! {message}", ex.Message);
      OnRealmEvent(new RealmErrorEvent()
      {
        Message = $"Failed to connect to realm server! {ex.Message}"
      });
    }
  }

  /// <summary>
  /// For the configured expansion returns a command object.
  /// </summary>
  /// <typeparam name="T">Packet Command Type</typeparam>
  /// <param name="commandId"></param>
  /// <returns></returns>
  public T GetCommand<T>()
    where T : IPacketCommand<RealmEvent>
  {
    var result = _packetCommands.Values.FirstOrDefault(command => typeof(T).IsInstanceOfType(command));
    if (result == null)
      throw new InvalidOperationException($"Unable to locate a packet command for {typeof(T)}");

    return (T)result;
  }

  public async Task RunCommand<T>()
    where T : IPacketCommand<RealmEvent>
  {
    var command = GetCommand<T>();
    await SendCommand(command);
  }

  public async Task SendCommand<T>(T command)
    where T : IPacketCommand<RealmEvent>
  {
    if (command == null) throw new ArgumentNullException(nameof(command));

    if (_realmChannel == null || _realmChannel.Active == false)
      throw new InvalidOperationException("A realm connection has not been established or is terminated.");

    var packet = await command.CreateCommandPacket(_realmChannel.Allocator);
    await _realmChannel.WriteAndFlushAsync(packet);
  }

  public async Task Disconnect()
  {
    if (_group.IsShuttingDown || _group.IsShutdown || _group.IsTerminated || _realmChannel == null)
    {
      return;
    }

    await _realmChannel.DisconnectAsync();
    await _realmChannel.CloseAsync();
    _realmChannel = null;
  }

  #region IObservable<RealmEvent>
  private readonly ConcurrentDictionary<IObserver<RealmEvent>, RealmEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<RealmEvent>, RealmEventUnsubscriber>();

  IDisposable IObservable<RealmEvent>.Subscribe(IObserver<RealmEvent> observer)
  {
    return _observers.GetOrAdd(observer, new RealmEventUnsubscriber(this, observer));
  }

  /// <summary>
  /// Produces Realm Events
  /// </summary>
  /// <param name="realmEvent"></param>
  private void OnRealmEvent(RealmEvent realmEvent)
  {
    Parallel.ForEach(_observers.Keys, (observer) =>
    {
      try
      {
        observer.OnNext(realmEvent);
      }
      catch (Exception)
      {
        // Do Nothing.
      }
    });
  }

  private sealed class RealmEventUnsubscriber : IDisposable
  {
    private readonly RealmConnector _parent;
    private readonly IObserver<RealmEvent> _observer;

    public RealmEventUnsubscriber(RealmConnector parent, IObserver<RealmEvent> observer)
    {
      _parent = parent ?? throw new ArgumentNullException(nameof(parent));
      _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public void Dispose()
    {
      if (_observer != null && _parent._observers.ContainsKey(_observer))
      {
        _parent._observers.TryRemove(_observer, out _);
      }
    }
  }
  #endregion
}
