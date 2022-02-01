﻿namespace WoWChat.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Realm;
using Realm.Events;
using Realm.PacketCommands;

public partial class WoWChat : IObserver<RealmEvent>
{
  private RealmConnector? _realmConnector;
  private IDisposable? _realmConnectorObserver;
  private IDisposable? _realmPacketHandlerObserver;

  public async Task ConnectLogonServer()
  {
    _realmConnector = _serviceProvider.GetRequiredService<RealmConnector>();

    _realmConnectorObserver = ((IObservable<RealmEvent>)_realmConnector).Subscribe(this);
    _realmPacketHandlerObserver = ((IObservable<RealmEvent>)_realmConnector.RealmPacketHandler).Subscribe(this);

    await _realmConnector.Connect();
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
        _logger.LogInformation("Successfully logged into logon server. Looking for realm {realmName}", _options.WoW.RealmName);
        _sessionKey = authenticatedEvent.SessionKey;
        _realmConnector?.RunCommand<RealmListCommand>().Wait();
        break;
      case RealmListEvent listEvent:
        var configRealm = _options.WoW.RealmName;
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

    //Re-Publish the event to any WoWChat observers
    OnWoWChatEvent(value);
  }
  #endregion
}
