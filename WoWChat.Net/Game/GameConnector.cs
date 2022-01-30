namespace WoWChat.Net.Game;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Collections.Concurrent;

public class GameConnector : IObservable<GameEvent>
{
  private readonly GameChannelInitializer _channelInitializer;
  private readonly IEventLoopGroup _group;
  private readonly WowChatOptions _options;
  private readonly ILogger<GameConnector> _logger;

  private GameRealm? _realm;
  private byte[]? _sessionKey;
  private IChannel? _gameChannel;

  public GameConnector(
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
  }

  public async Task Connect(GameRealm gameRealm, byte[] sessionKey)
  {
    if (_gameChannel != null && _gameChannel.Active)
    {
      throw new InvalidOperationException("Refusing to connect to game server. Already connected.");
    }

    _realm = gameRealm ?? throw new ArgumentNullException(nameof(gameRealm));
    _sessionKey = sessionKey ?? throw new ArgumentNullException(nameof(sessionKey));
    _channelInitializer.SetConnectionOptions(_realm, _sessionKey);

    OnGameEvent(new GameConnectingEvent()
    {
      Realm = _realm,
    });

    var bootstrap = new Bootstrap();
    bootstrap.Group(_group)
      .Channel<TcpSocketChannel>()
      .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_options.ConnectTimeoutMs))
      .Option(ChannelOption.SoKeepalive, true)
      .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
      .RemoteAddress(_realm.Host, _realm.Port)
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
        Realm = _realm
      });
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to connect to game server! {message}", ex.Message);
      OnGameEvent(new GameDisconnectedEvent()
      {
        Reason = $"Failed to connect to game server! {ex.Message}"
      });
    }
  }

  public Task Disconnect()
  {
    return Task.CompletedTask;
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
