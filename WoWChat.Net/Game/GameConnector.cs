namespace WoWChat.Net.Game;

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
using System.Collections.Concurrent;

public partial class GameConnector : IObservable<GameEvent>
{
  private readonly GameChannelInitializer _channelInitializer;
  private readonly IEventLoopGroup _group;
  private readonly WowChatOptions _options;
  private readonly ILogger<GameConnector> _logger;

  protected IDictionary<int, IPacketCommand<GameEvent>> _packetCommands = new Dictionary<int, IPacketCommand<GameEvent>>();

  private GameServerInfo? _gameServer;
  private IChannel? _gameChannel;

  public GameConnector(
    IServiceProvider serviceProvider,
    GameChannelInitializer gameChannelInitializer,
    IEventLoopGroup group,
    IOptionsSnapshot<WowChatOptions> options,
    ILogger<GameConnector> logger
    )
  {
    _channelInitializer = gameChannelInitializer ?? throw new ArgumentNullException(nameof(gameChannelInitializer));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _group = group ?? throw new ArgumentNullException(nameof(group));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    InitPacketCommands(serviceProvider, _options.WoW.GetExpansion());
  }

  public bool IsConnected { get {  return _gameChannel != null && _gameChannel.Active; } }

  public IEventLoopGroup Group { get { return _group; } }

  public GamePacketHandler GamePacketHandler { get { return _channelInitializer.GamePacketHandler; } }

  /// <summary>
  /// Binds the game packet commands defined by the service provider. Called by the constructor
  /// </summary>
  /// <param name="serviceProvider"></param>
  /// <exception cref="InvalidOperationException"></exception>
  private void InitPacketCommands(IServiceProvider serviceProvider, WoWExpansion expansion)
  {
    // Init PacketCommands
    var packetCommandType = typeof(IPacketCommand<GameEvent>);
    var packetCommandAttributeType = typeof(PacketCommandAttribute);
    var handlerTypes = serviceProvider.GetServices(packetCommandType)
      .Select(o => o?.GetType())
      .Where(p => packetCommandType.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetCommandAttributeType));

    var packetCommandGameEventCallback = new Action<GameEvent>((gameEvent) => OnGameEvent(gameEvent));

    foreach (var handlerType in handlerTypes)
    {
      if (handlerType == default) continue;

      var packetCommandAttributes = handlerType.GetCustomAttributes(false).OfType<PacketCommandAttribute>();

      foreach (var packetCommandAttribute in packetCommandAttributes)
      {
        if ((packetCommandAttribute.Expansion & expansion) != expansion)
        {
          _logger.LogDebug("Skipping {handlerType} for {targetExpansion} (indicates {packetHandlerExpansion})", handlerType, expansion, packetCommandAttribute.Expansion);
          continue;
        }

        if (_packetCommands.ContainsKey(packetCommandAttribute.Id))
        {
          throw new InvalidOperationException($"A packet command is already registered for packet id {packetCommandAttribute.Id} ({BitConverter.ToString(packetCommandAttribute.Id.ToBytes())}), expansion: {packetCommandAttribute.Expansion}");
        }

        var packetCommand = (IPacketCommand<GameEvent>)serviceProvider.GetRequiredService(handlerType);
        packetCommand.CommandId = packetCommandAttribute.Id;
        packetCommand.EventCallback = packetCommandGameEventCallback;

        _packetCommands.Add(packetCommandAttribute.Id, packetCommand);
      }
    }
  }

  public async Task Connect(GameServerInfo gameServer, SessionInfo session)
  {
    if (gameServer == null)
    {
      throw new ArgumentNullException(nameof(gameServer));
    }

    if (session == null || session.SessionKey == null || session.SessionKey.Length != 40)
    {
      throw new ArgumentOutOfRangeException(nameof(session));
    }

    if (_gameChannel != null && _gameChannel.Active)
    {
      throw new InvalidOperationException("Refusing to connect to game server. Already connected.");
    }

    _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
    _channelInitializer.SetConnectionOptions(_gameServer, session);

    OnGameEvent(new GameConnectingEvent()
    {
      GameServer = _gameServer,
    });

    var bootstrap = new Bootstrap();
    bootstrap.Group(_group)
      .Channel<TcpSocketChannel>()
      .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_options.ConnectTimeoutMs))
      .Option(ChannelOption.SoKeepalive, true)
      .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
      .RemoteAddress(_gameServer.Host, _gameServer.Port)
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

      _gameChannel = connectTask.Result;
      OnGameEvent(new GameConnectedEvent()
      {
        GameServerInfo = _gameServer
      });
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to connect to game server! {message}", ex.Message);
      OnGameEvent(new GameErrorEvent()
      {
        Message = $"Failed to connect to game server! {ex.Message}"
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
    where T : IPacketCommand<GameEvent>
  {
    var result = _packetCommands.Values.FirstOrDefault(command => typeof(T).IsInstanceOfType(command));
    if (result == null)
      throw new InvalidOperationException($"Unable to locate a packet command for {typeof(T)}");

    return (T)result;
  }

  public async Task RunCommand<T>()
    where T : IPacketCommand<GameEvent>
  {
    var command = GetCommand<T>();
    await SendCommand(command);
  }

  public async Task SendCommand<T>(T command)
    where T : IPacketCommand<GameEvent>
  {
    if (command == null) throw new ArgumentNullException(nameof(command));

    if (_gameChannel == null || _gameChannel.Active == false)
      throw new InvalidOperationException("A game connection has not been established or is terminated.");

    var packet = await command.CreateCommandPacket(_gameChannel.Allocator);
    await _gameChannel.WriteAndFlushAsync(packet);
  }

  public async Task Disconnect()
  {
    if (_group.IsShuttingDown || _group.IsShutdown || _group.IsTerminated || _gameChannel == null)
    {
      return;
    }

    await _gameChannel.DisconnectAsync();
    _gameChannel = null;
  }

  #region IObservable<GameEvent>
  private readonly ConcurrentDictionary<IObserver<GameEvent>, GameEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<GameEvent>, GameEventUnsubscriber>();

  IDisposable IObservable<GameEvent>.Subscribe(IObserver<GameEvent> observer)
  {
    return _observers.GetOrAdd(observer, new GameEventUnsubscriber(this, observer));
  }

  /// <summary>
  /// Produces Game Events
  /// </summary>
  /// <param name="gameEvent"></param>
  private void OnGameEvent(GameEvent gameEvent)
  {
    Parallel.ForEach(_observers.Keys, (observer) =>
    {
      try
      {
        observer.OnNext(gameEvent);
      }
      catch (Exception)
      {
        // Do Nothing.
      }
    });
  }

  private sealed class GameEventUnsubscriber : IDisposable
  {
    private readonly GameConnector _parent;
    private readonly IObserver<GameEvent> _observer;

    public GameEventUnsubscriber(GameConnector parent, IObserver<GameEvent> observer)
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