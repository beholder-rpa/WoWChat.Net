namespace WoWChat.Net
{
  using DotNetty.Transport.Channels;
  using Extensions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using Realm;
  using Realm.Events;
  using System;

  public class WoWChat : IWoWChat, IObserver<RealmEvent>
  {
    private readonly WowChatOptions _options;
    private readonly IEventLoopGroup _group;
    private readonly RealmConnector _realmConnector;
    private readonly RealmPacketHandler _realmPacketHandler;
    private readonly ILogger<WoWChat> _logger;

    public WoWChat(IOptions<WowChatOptions> options, IEventLoopGroup group, RealmConnector realmConnector, RealmPacketHandler realmPacketHandler, ILogger<WoWChat> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _group = group ?? throw new ArgumentNullException(nameof(group));
      _realmConnector = realmConnector ?? throw new ArgumentNullException(nameof(realmConnector));
      _realmPacketHandler = realmPacketHandler ?? throw new ArgumentNullException(nameof(realmPacketHandler));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Run(CancellationToken cancellationToken)
    {
      ((IObservable<RealmEvent>)_realmConnector).Subscribe(this);
      ((IObservable<RealmEvent>)_realmPacketHandler).Subscribe(this);

      await _realmConnector.Connect();
    }

    void IObserver<RealmEvent>.OnCompleted()
    {
      _logger.LogInformation("RealmEvent Observable Completed.");
    }

    void IObserver<RealmEvent>.OnError(Exception error)
    {
      _logger.LogError("An unexpected error occurred: {message}", error.Message);
    }

    void IObserver<RealmEvent>.OnNext(RealmEvent value)
    {
      switch (value)
      {
        case RealmConnectedEvent connectedEvent:
          _logger.LogInformation("Connected! Sending account login information...");
          break;
        case RealmAuthenticatedEvent authenticatedEvent:
          _logger.LogInformation("Successfully logged into realm server. Looking for realm {realmName}", _options.RealmName);
          break;
        case RealmListEvent listEvent:
          var configRealm = _options.RealmName;
          var realmList = listEvent.RealmList;
          var realm = realmList.FirstOrDefault(realm => string.Equals(realm.Name, configRealm, StringComparison.CurrentCultureIgnoreCase));

          if (realm == null)
          {
            _logger.LogError("Realm {realm} not found!", configRealm);
            _logger.LogError("{realmCount} possible realms:", realmList.Count);
            foreach (var availableRealm in realmList)
            {
              _logger.LogError("\t{realmName}", availableRealm.Name);
            }
          }
          else
          {
            _logger.LogInformation("Successfully located #{realmId} - {realmName} at {host}:{port}", realm.RealmId, _options.RealmName, realm.Host, realm.Port);
          }
          break;
        case RealmDisconnectedEvent disconnectedEvent when disconnectedEvent.AutoReconnect == true:
          _group.ShutdownGracefullyAsync().Wait();
          Task.Delay(_options.ReconnectDelayMs).Wait();
          _logger.LogInformation("Disconnected from server! Reconnecting in {reconnectDelay} seconds...", _options.ReconnectDelayMs);
          _realmConnector.Connect().Forget();
          break;
        case RealmDisconnectedEvent disconnectedEvent when disconnectedEvent.AutoReconnect == false:
          if (disconnectedEvent.IsExpected)
            _logger.LogInformation("Expected disconnect from realm server.");
          else
            _logger.LogInformation("Unexpected disconnect from realm server. Reason: {reason}", disconnectedEvent.Reason);
          break;
        case RealmErrorEvent errorEvent:
          _logger.LogInformation("Error: {message}", errorEvent.Message);
          break;
      }
    }
  }
}
