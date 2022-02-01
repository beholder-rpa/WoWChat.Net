namespace WoWChat.Net;

using Common;
using Extensions;
using Game;
using Game.Events;
using Game.PacketCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Timers;

public partial class WoWChat : IObserver<GameEvent>
{
  private readonly GameNameLookup _nameLookup;
  private byte[] _sessionKey = Array.Empty<byte>();
  private GameServerInfo? _selectedGameServer;
  private GameCharacter? _selectedCharacter;
  private bool _inWorld = false;

  private GameConnector? _gameConnector;
  private IDisposable? _gameConnectorObserver;
  private IDisposable? _gamePacketHandlerObserver;

  // Timers
  protected readonly Timer _pingTimer;
  protected readonly Timer _keepAliveTimer;

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
        _selectedCharacter = retrievedCharacters.Characters.FirstOrDefault(c => c.Name.ToLowerInvariant() == _options.WoW.CharacterName.ToLowerInvariant());
        if (_selectedCharacter == null)
        {
          _logger.LogError("Character {character} was not found!", _options.WoW.CharacterName);
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
        if (_options.WoW.GetExpansion() >= WoWExpansion.TBC)
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

    //Re-Publish the event to any WoWChat observers
    OnWoWChatEvent(value);
  }
  #endregion
}