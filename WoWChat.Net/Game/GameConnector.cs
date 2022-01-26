namespace WoWChat.Net.Game;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Networking;
using Options;
using System.Net.Sockets;

public class GameConnector
{
  private readonly WowChatOptions _gameOptions;
  private readonly ILogger<GameConnector> _logger;
  private readonly TcpClient _tcpClient;

  private readonly IGamePacketEncoder _gamePacketEncoder;
  private readonly IGamePacketDecoder _gamePacketDecoder;
  private readonly IGameHeaderCrypt _gameHeaderCrypt;

  public GameConnector(
    IGamePacketEncoder gamePacketEncoder,
    IGamePacketDecoder gamePacketDecoder,
    IGameHeaderCrypt gameHeaderCrypt,
    IOptions<WowChatOptions> gameOptions,
    ILogger<GameConnector> logger,
    string realmServer
    )
  {
    _gameOptions = gameOptions?.Value ?? throw new ArgumentNullException(nameof(gameOptions));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _tcpClient = new TcpClient(realmServer, _gameOptions.RealmListPort);

    _gamePacketEncoder = gamePacketEncoder;
    _gamePacketDecoder = gamePacketDecoder;
    _gameHeaderCrypt = gameHeaderCrypt;
  }

  public async Task Connect()
  {
    if (_tcpClient.Connected)
    {
      throw new InvalidOperationException("Refusing to connect to game server. Already connected.");
    }

    _logger.LogInformation("Connecting to game server {realmName} ({host}:{port})", _gameOptions.RealmName, _gameOptions.RealmListHost, _gameOptions.RealmListPort);

    var cancelTask = Task.Delay(_gameOptions.ConnectTimeoutMs);
    var connectTask = _tcpClient.ConnectAsync(_gameOptions.RealmListHost, _gameOptions.RealmListPort);

    //double await so if cancelTask throws exception, this throws it
    await await Task.WhenAny(connectTask, cancelTask);

    if (cancelTask.IsCompleted)
    {
      //If cancelTask and connectTask both finish at the same time,
      //we'll consider it to be a timeout. 
      throw new TimeoutException("Failed to connect to game server! Connection Timeout");
    }
  }

  public Task Disconnect()
  {
    return Task.CompletedTask;
  }

  private void OnDisconnect()
  {

  }
}
