namespace WoWChat.Net;

using Common;
using Extensions;
using Game;
using Game.Events;
using Game.PacketCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Options;

using System.Timers;

public partial class WoWChat : IObserver<GameEvent>
{
  private readonly GameChannelLookup _channelLookup;
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
    _channelLookup.Clear();
    _nameLookup.Clear();

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

  protected virtual async Task Reconnect()
  {
    await DisconnectGameServer();
    await DisconnectLogonServer();
    if (!_cancellationToken.IsCancellationRequested)
    {
      _logger.LogInformation("Disconnected from game server! Reconnecting in {reconnectDelay} seconds...", TimeSpan.FromMilliseconds(_options.ReconnectDelayMs));
      Task.Delay(_options.ReconnectDelayMs).Wait();
      ConnectLogonServer().Wait();
    }
  }

  /// <summary>
  /// Joins the channels contained in the specified ChatOptions object
  /// </summary>
  /// <param name="options"></param>
  /// <returns></returns>
  protected virtual Task JoinChatChannels(ChatOptions options)
  {
    if (options.Enabled == false)
    {
      _logger.LogInformation("Chat is not enabled.");
      return Task.CompletedTask;
    }

    for (int i = 0; i < options.Channels.Length; i++)
    {
      var channel = options.Channels[i];

      if (channel.Enabled == false)
        continue;

      var joinChannelCommand = _gameConnector?.GetCommand<JoinChannelCommand>();
      if (joinChannelCommand != null)
      {
        joinChannelCommand.ChannelId = i;
        joinChannelCommand.ChannelName = channel.Name;
        _gameConnector?.SendCommand(joinChannelCommand);
      }
    }

    return Task.CompletedTask;
  }

  #region IObserver<GameEvent>
  void IObserver<GameEvent>.OnCompleted()
  {
    _logger.LogInformation("GameEvent Observable Completed.");
  }

  void IObserver<GameEvent>.OnError(Exception error)
  {
    _logger.LogError("An unexpected error occurred: {message}", error.Message);
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
        JoinChatChannels(_options.WoW.Chat).Wait();
        break;
      case GameChatMessageEvent chatMessageEvent:
        var msg = chatMessageEvent.ChatMessage;
        _logger.LogInformation("Chat Message: {formattedMessage}", msg.FormattedMessage);
        break;
      case GameChannelNotificationEvent channelNotification:
        _logger.LogInformation("Channel Notification: {channelName} - {kind}", channelNotification.ChannelName, channelNotification.Kind);
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
      case GameServerMessageEvent serverMessageEvent:
        switch (serverMessageEvent.Kind)
        {
          case ServerMessageKind.SERVER_MSG_SHUTDOWN_TIME:
            _logger.LogInformation("Server Message: Shutdown in {shutdownTime}", serverMessageEvent.Message);
            break;
          case ServerMessageKind.SERVER_MSG_RESTART_TIME:
            _logger.LogInformation("Server Message: Restart in {shutdownTime}", serverMessageEvent.Message);
            break;
          case ServerMessageKind.SERVER_MSG_SHUTDOWN_CANCELLED:
            _logger.LogInformation("Server Message: Shutdown Cancelled. {reason}", serverMessageEvent.Message);
            break;
          case ServerMessageKind.SERVER_MSG_RESTART_CANCELLED:
            _logger.LogInformation("Server Message: Restart Cancelled. {reason}", serverMessageEvent.Message);
            break;
          case ServerMessageKind.SERVER_MSG_CUSTOM:
          default:
            _logger.LogInformation("Server Message: {message} {kind} ", serverMessageEvent.Message, serverMessageEvent.Kind);
            break;
        }
        break;
      case GameInvalidatePlayerEvent invalidatePlayerEvent:
        _nameLookup.Remove(invalidatePlayerEvent.PlayerId, out _);
        break;
      case GameDisconnectedEvent disconnectEvent:
        _logger.LogInformation("Game Server Disconnected.");
        Reconnect().Wait();
        break;
      case GameErrorEvent errorEvent:
        _logger.LogInformation("Game Server Error: {message}", errorEvent.Message);
        Reconnect().Wait();
        break;
      default:
        _logger.LogWarning("Warning: Unhandled Game Event: {eventType}", value.GetType());
        // TODO: Some sort of tracking of unhandled game events
        break;
    }

    //Re-Publish the event to any WoWChat observers
    OnWoWChatEvent(value);
  }
  #endregion
}