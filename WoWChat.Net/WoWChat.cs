namespace WoWChat.Net
{
  using Common;
  using Extensions;
  using Game;
  using Game.Events;
  using Game.PacketCommands;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using Realm;
  using Realm.Events;
  using Realm.PacketCommands;
  using System;
  using System.Timers;

  public class WoWChat : IWoWChat, IObserver<RealmEvent>, IObserver<GameEvent>
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly GameNameLookup _nameLookup;
    private readonly WowChatOptions _options;

    private readonly ILogger<WoWChat> _logger;

    private CancellationToken _cancellationToken;

    private RealmConnector? _realmConnector;
    private IDisposable? _realmConnectorObserver;
    private IDisposable? _realmPacketHandlerObserver;

    private GameConnector? _gameConnector;
    private IDisposable? _gameConnectorObserver;
    private IDisposable? _gamePacketHandlerObserver;

    // Timers
    protected readonly Timer _pingTimer;
    protected readonly Timer _keepAliveTimer;

    private byte[] _sessionKey = Array.Empty<byte>();
    private GameServerInfo? _selectedGameServer;
    private GameCharacter? _selectedCharacter;
    private bool _inWorld = false;

    public WoWChat(
      IServiceProvider serviceProvider,
      GameNameLookup nameLookup,
      IOptionsSnapshot<WowChatOptions> options,
      ILogger<WoWChat> logger)
    {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
    }

    protected virtual void RunPingExecutor(object? sender, ElapsedEventArgs e)
    {
      if (_gameConnector == null)
      {
        _logger.LogDebug("Attempted to ping while the game connector is not open. Stopping ping timer.");
        _pingTimer.Enabled = false;
        return;
      }

      _gameConnector.RunCommand<PingCommand>().Forget();
    }

    protected virtual void RunKeepAliveExecutor(object? sender, ElapsedEventArgs e)
    {
      if (_gameConnector == null)
      {
        _logger.LogDebug("Attempted to keep alive while the game connector is not open. Stopping ping timer.");
        _keepAliveTimer.Enabled = false;
        return;
      }

      _gameConnector.RunCommand<KeepAliveCommand>().Forget();
    }

    public async Task Run(CancellationToken cancellationToken)
    {
      _cancellationToken = cancellationToken;
      await ConnectLogonServer();
      await Task.Delay(-1, cancellationToken);
      await DisconnectLogonServer();
    }

    public async Task ConnectLogonServer()
    {
      _realmConnector = _serviceProvider.GetRequiredService<RealmConnector>();

      _realmConnectorObserver = ((IObservable<RealmEvent>)_realmConnector).Subscribe(this);
      _realmPacketHandlerObserver = ((IObservable<RealmEvent>)_realmConnector.RealmPacketHandler).Subscribe(this);

      await _realmConnector.Connect();
    }

    public async Task ConnectGameServer(GameServerInfo gameServer, byte[] sessionKey)
    {
      if (gameServer == null)
      {
        throw new ArgumentNullException(nameof(gameServer));
      }

      if (sessionKey == null || sessionKey.Length != 40)
      {
        throw new ArgumentNullException(nameof(sessionKey));
      }

      _gameConnector = _serviceProvider.GetRequiredService<GameConnector>();

      _gameConnectorObserver = ((IObservable<GameEvent>)_gameConnector).Subscribe(this);
      _gamePacketHandlerObserver = ((IObservable<GameEvent>)_gameConnector.GamePacketHandler).Subscribe(this);

      await _gameConnector.Connect(gameServer, sessionKey);
    }

    public async Task DisconnectLogonServer()
    {
      _logger.LogDebug("Disconnecting from logon server...");
      if (_realmPacketHandlerObserver != null)
      {
        _realmPacketHandlerObserver.Dispose();
        _realmPacketHandlerObserver = null;
      }

      if (_realmConnectorObserver != null)
      {
        _realmConnectorObserver.Dispose();
        _realmConnectorObserver = null;
      }

      if (_realmConnector != null)
      {
        await _realmConnector.Disconnect();
        _realmConnector = null;
      }

      _logger.LogDebug("Disconnected from logon server.");
    }

    public async Task DisconnectGameServer()
    {
      _logger.LogDebug("Disconnecting from game server...");

      _inWorld = false;
      _pingTimer.Stop();
      _keepAliveTimer.Stop();

      if (_gamePacketHandlerObserver != null)
      {
        _gamePacketHandlerObserver.Dispose();
        _gamePacketHandlerObserver = null;
      }

      if (_gameConnectorObserver != null)
      {
        _gameConnectorObserver.Dispose();
        _gameConnectorObserver = null;
      }

      if (_gameConnector != null)
      {
        await _gameConnector.Disconnect();
        _gameConnector = null;
      }

      _logger.LogDebug("Disconnected from game server.");
    }

    #region IObserver<RealmEvent>
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
        case RealmConnectingEvent connectingEvent:
          _logger.LogInformation("Connecting to logon server for {realmName} at {host}:{port}", connectingEvent.Name, connectingEvent.Host, connectingEvent.Port);
          break;
        case RealmConnectedEvent connectedEvent:
          _logger.LogInformation("Connected to logon server! Sending account login information...");
          break;
        case RealmAuthenticatedEvent authenticatedEvent:
          _logger.LogInformation("Successfully logged into logon server. Looking for realm {realmName}", _options.RealmName);
          _sessionKey = authenticatedEvent.SessionKey;
          _realmConnector?.RunCommand<RealmListCommand>().Wait();
          break;
        case RealmListEvent listEvent:
          var configRealm = _options.RealmName;
          var realmList = listEvent.RealmList;

          _logger.LogInformation("Retrieved {realmCount} realms.", realmList.Count);
          var realm = realmList.FirstOrDefault(realm => string.Equals(realm.Name, configRealm, StringComparison.CurrentCultureIgnoreCase));

          if (realm == null)
          {
            _logger.LogError("Realm {realm} not found!", configRealm);
            _logger.LogError("{realmCount} possible realms:", realmList.Count);
            foreach (var availableRealm in realmList)
            {
              _logger.LogError("\t{realmName}", availableRealm.Name);
            }
            DisconnectLogonServer().Wait();
          }
          else
          {
            _selectedGameServer = realm;
            _logger.LogInformation("Successfully located #{realmId} - {realmName} at {host}:{port} ({version})", realm.RealmId, realm.Name, realm.Host, realm.Port, realm.Version == null ? "Unknown Version" : realm.Version.ToString());
            ConnectGameServer(_selectedGameServer, _sessionKey).Wait();
            DisconnectLogonServer().Wait();
          }
          break;
        case RealmDisconnectedEvent disconnectedEvent when _selectedGameServer == null:
          DisconnectLogonServer().Wait();
          if (!_cancellationToken.IsCancellationRequested)
          {
            _logger.LogInformation("Disconnected from logon server! Reconnecting in {reconnectDelay} seconds...", TimeSpan.FromMilliseconds(_options.ReconnectDelayMs));
            Task.Delay(_options.ReconnectDelayMs).Wait();
            ConnectLogonServer().Wait();
          }
          break;
        case RealmDisconnectedEvent disconnectedEvent when _selectedGameServer != null:
          _logger.LogInformation("Disconnected from logon server. Realm game server located.");
          break;
        case RealmErrorEvent errorEvent:
          _logger.LogInformation("Logon Server Error: {message}", errorEvent.Message);
          DisconnectLogonServer().Wait();
          if (!_cancellationToken.IsCancellationRequested)
          {
            _logger.LogInformation("Disconnected from logon server! Reconnecting in {reconnectDelay} seconds...", TimeSpan.FromMilliseconds(_options.ReconnectDelayMs));
            Task.Delay(_options.ReconnectDelayMs).Wait();
            ConnectLogonServer().Wait();
          }
          break;
        default:
          _logger.LogWarning("Warning: Unhandled Realm Event: {eventType}", value.GetType());
          break;
      }
    }
    #endregion

    #region IObserver<GameEvent>
    void IObserver<GameEvent>.OnCompleted()
    {
      throw new NotImplementedException();
    }

    void IObserver<GameEvent>.OnError(Exception error)
    {
      throw new NotImplementedException();
    }

    void IObserver<GameEvent>.OnNext(GameEvent value)
    {
      switch (value)
      {
        case GameConnectingEvent connectingEvent:
          _logger.LogInformation("Connecting to game server {realmName} ({host}:{port})", connectingEvent.GameServer.Name, connectingEvent.GameServer.Host, connectingEvent.GameServer.Port);
          break;
        case GameConnectedEvent:
          _logger.LogInformation("Connected! Authenticating...");
          break;
        case GameLoggedInEvent:
          _logger.LogInformation("Successfully logged in!");
          _pingTimer.Start();
          _gameConnector?.RunCommand<EnumerateCharactersCommand>().Wait();
          break;
        case GameRetrievedCharactersEvent retrievedCharacters:
          _logger.LogInformation("Retrieved {numCharacters} characters.", retrievedCharacters.Characters.Count);
          _selectedCharacter = retrievedCharacters.Characters.FirstOrDefault(c => c.Name.ToLowerInvariant() == _options.CharacterName.ToLowerInvariant());
          if (_selectedCharacter == null)
          {
            _logger.LogError("Character {character} was not found!", _options.CharacterName);
            DisconnectGameServer().Wait();
            DisconnectLogonServer().Wait();
          }
          else
          {
            _logger.LogInformation("Logging in with character {character}...", _selectedCharacter.Name);
            var loginCommand = _gameConnector?.GetCommand<PlayerLoginCommand>();
            if (loginCommand != null)
            {
              loginCommand.Character = _selectedCharacter;
              _gameConnector?.SendCommand(loginCommand);
            }
          }
          break;
        case GameJoinedWorldEvent joinedWorldEvent:
          if (_inWorld)
          {
            return;
          }
          _logger.LogInformation("Successfully joined the world!");
          _inWorld = true;
          if (_options.GetExpansion() >= WoWExpansion.TBC)
          {
            _keepAliveTimer.Start();
          }
          // TODO: Join Channels
          break;
        case GameChatMessageEvent chatMessageEvent:
          var msg = chatMessageEvent.ChatMessage;
          _logger.LogInformation(msg.FormattedMessage);
          break;
        case GameNameQueryEvent nameQueryEvent:
          _nameLookup.AddOrUpdate(nameQueryEvent.NameQuery);
          break;
        case GameNameQueryRequestEvent nameQueryRequestEvent:
          var command = _gameConnector?.GetCommand<GameNameQueryCommand>();
          if (command != null)
          {
            command.Guid = nameQueryRequestEvent.Guid;
            _gameConnector?.SendCommand(command).Forget();
          }
          break;
        case GameInvalidatePlayerEvent invalidatePlayerEvent:
          _nameLookup.Remove(invalidatePlayerEvent.PlayerId, out _);
          break;
        case GameErrorEvent errorEvent:
          _logger.LogInformation("Game Server Error: {message}", errorEvent.Message);
          DisconnectGameServer().Wait();
          DisconnectLogonServer().Wait();
          if (!_cancellationToken.IsCancellationRequested)
          {
            _logger.LogInformation("Disconnected from game server! Reconnecting in {reconnectDelay} seconds...", TimeSpan.FromMilliseconds(_options.ReconnectDelayMs));
            Task.Delay(_options.ReconnectDelayMs).Wait();
            ConnectLogonServer().Wait();
          }
          break;
        default:
          _logger.LogWarning("Warning: Unhandled Game Event: {eventType}", value.GetType());
          break;
      }
    }
    #endregion
  }
}
