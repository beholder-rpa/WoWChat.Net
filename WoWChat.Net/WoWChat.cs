namespace WoWChat.Net
{
  using Common;
  using DotNetty.Transport.Channels;
  using Extensions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Concurrent;
  using System.Timers;

  /// <summary>
  /// WoWChat implementation
  /// </summary>
  public partial class WoWChat : IWoWChat, IDisposable
  {
    private readonly IServiceProvider _serviceProvider;

    private readonly IEventLoopGroup _group;

    private readonly WowChatOptions _options;

    private readonly ILogger<WoWChat> _logger;

    private CancellationToken _cancellationToken;

    public WoWChat(
      IServiceProvider serviceProvider,
      IEventLoopGroup group,
      GameChannelLookup channelLookup,
      GameNameLookup nameLookup,
      IOptionsSnapshot<WowChatOptions> options,
      ILogger<WoWChat> logger)
    {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
      _group = group ?? throw new ArgumentNullException(nameof(group));
      _channelLookup = channelLookup ?? throw new ArgumentNullException(nameof(channelLookup));
      _nameLookup = nameLookup ?? throw new ArgumentNullException(nameof(nameLookup));
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _pingTimer = new Timer(30 * 1000)
      {
        AutoReset = true,
        Enabled = false,
      };
      _pingTimer.Elapsed += RunPingExecutor;

      _keepAliveTimer = new Timer(30 * 1000)
      {
        AutoReset = true,
        Enabled = false,
      };
      _keepAliveTimer.Elapsed += RunKeepAliveExecutor;

      _ensureJoinedWorldAfterConnectTimer = new Timer(30 * 1000)
      {
        AutoReset = false,
        Enabled = false,
      };
      _ensureJoinedWorldAfterConnectTimer.Elapsed += RunFailedToJoinWorldExecutor;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
      _cancellationToken = cancellationToken;
      await ConnectLogonServer();
    }

    protected virtual async Task Reconnect()
    {
      _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(2000)).Wait(2500);
      DisconnectGameServer().Wait(2000);
      DisconnectLogonServer().Wait(2000);
      if (!_cancellationToken.IsCancellationRequested)
      {
        _logger.LogInformation("Disconnected from game server! Reconnecting in {reconnectDelay} seconds...", TimeSpan.FromMilliseconds(_options.ReconnectDelayMs));
        await Task.Delay(_options.ReconnectDelayMs);
        ConnectLogonServer().Wait();
      }
    }

    #region IObservable<IWoWChatEvent>
    private readonly ConcurrentDictionary<IObserver<IWoWChatEvent>, WoWChatEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<IWoWChatEvent>, WoWChatEventUnsubscriber>();

    IDisposable IObservable<IWoWChatEvent>.Subscribe(IObserver<IWoWChatEvent> observer)
    {
      return _observers.GetOrAdd(observer, new WoWChatEventUnsubscriber(this, observer));
    }

    /// <summary>
    /// Produces WowChat Events - Called by Realm and Game suscribers
    /// </summary>
    /// <param name="realmEvent"></param>
    private void OnWoWChatEvent(IWoWChatEvent wowChatEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(wowChatEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    private sealed class WoWChatEventUnsubscriber : IDisposable
    {
      private readonly WoWChat _parent;
      private readonly IObserver<IWoWChatEvent> _observer;

      public WoWChatEventUnsubscriber(WoWChat parent, IObserver<IWoWChatEvent> observer)
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

    #region IDisposable
    private bool _isDisposed;
    protected virtual void Dispose(bool disposing)
    {
      if (!_isDisposed)
      {
        if (disposing)
        {
          DisconnectGameServer().Forget();
          DisconnectLogonServer().Forget();
        }

        _isDisposed = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}