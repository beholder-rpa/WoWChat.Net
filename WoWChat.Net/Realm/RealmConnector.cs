namespace WoWChat.Net.Realm;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Events;
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

  private IChannel? _realmChannel;

  public RealmConnector(RealmChannelInitializer realmChannelInitializer, IEventLoopGroup group, IOptions<WowChatOptions> options, ILogger<RealmConnector> logger)
  {
    _channelInitializer = realmChannelInitializer ?? throw new ArgumentNullException(nameof(realmChannelInitializer));
    _group = group ?? throw new ArgumentNullException(nameof(group));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
      OnRealmEvent(new RealmConnectedEvent(){
        Name = _options.RealmName,
        Host = _options.RealmListHost,
        Port = _options.RealmListPort
      });
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to connect to realm server! {message}", ex.Message);
      OnRealmEvent(new RealmDisconnectedEvent()
      {
        Reason = $"Failed to connect to realm server! {ex.Message}"
      });
    }
  }

  public async Task Disconnect()
  {
    if (_group.IsShuttingDown || _group.IsShutdown || _group.IsTerminated || _realmChannel == null)
    {
      return;
    }

    await _realmChannel.CloseAsync();
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
