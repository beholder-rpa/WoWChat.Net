namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Handlers.Timeout;
  using DotNetty.Transport.Channels;
  using DotNetty.Transport.Channels.Sockets;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;

  public class GameChannelInitializer : ChannelInitializer<TcpSocketChannel>
  {
    private readonly GamePacketHandler _gamePacketHandler;
    private readonly GamePacketDecoder _gamePacketDecoder;
    private readonly GamePacketEncoder _gamePacketEncoder;
    private readonly IdleStateCallback _idleStateCallback;

    protected readonly WowChatOptions _options;
    protected readonly ILogger<GameChannelInitializer> _logger;

    public GameChannelInitializer(
      GamePacketHandler gamePacketHandler,
      GamePacketDecoder gamePacketDecoder,
      GamePacketEncoder gamePacketEncoder,
      IdleStateCallback idleStateCallback,
      IOptions<WowChatOptions> options,
      ILogger<GameChannelInitializer> logger
      )
    {
      _gamePacketHandler = gamePacketHandler ?? throw new ArgumentNullException(nameof(gamePacketHandler));
      _gamePacketDecoder = gamePacketDecoder ?? throw new ArgumentNullException(nameof(gamePacketDecoder));
      _gamePacketEncoder = gamePacketEncoder ?? throw new ArgumentNullException(nameof(gamePacketEncoder));
      _idleStateCallback = idleStateCallback ?? throw new ArgumentNullException(nameof(idleStateCallback));
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void InitChannel(TcpSocketChannel channel)
    {
      channel.Pipeline.AddLast(
        new IdleStateHandler(60, 120, 0),
        _idleStateCallback,
        _gamePacketDecoder,
        _gamePacketEncoder,
        _gamePacketHandler
      );
    }
  }
}
