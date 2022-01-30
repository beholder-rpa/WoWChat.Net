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
      IOptionsSnapshot<WowChatOptions> options,
      GamePacketHandlerResolver gamePacketHandlerResolver,
      GamePacketDecoderResolver gamePacketDecoderResolver,
      GamePacketEncoderResolver gamePacketEncoderResolver,
      IdleStateCallback idleStateCallback,
      ILogger<GameChannelInitializer> logger
      )
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      var expansion = _options.GetExpansion();

      _gamePacketHandler = gamePacketHandlerResolver(expansion) ?? throw new ArgumentNullException(nameof(gamePacketHandlerResolver));
      _gamePacketDecoder = gamePacketDecoderResolver(expansion) ?? throw new ArgumentNullException(nameof(gamePacketDecoderResolver));
      _gamePacketEncoder = gamePacketEncoderResolver(expansion) ?? throw new ArgumentNullException(nameof(gamePacketEncoderResolver));
      _idleStateCallback = idleStateCallback ?? throw new ArgumentNullException(nameof(idleStateCallback));
     
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetConnectionOptions(GameRealm realm, byte[] sessionKey)
    {
      _gamePacketHandler.Realm = realm;
      _gamePacketHandler.SessionKey = sessionKey;
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
