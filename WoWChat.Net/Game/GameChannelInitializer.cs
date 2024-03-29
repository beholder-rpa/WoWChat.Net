﻿namespace WoWChat.Net.Game
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
      GamePacketHandler gamePacketHandler,
      GamePacketDecoderResolver gamePacketDecoderResolver,
      GamePacketEncoderResolver gamePacketEncoderResolver,
      IdleStateCallback idleStateCallback,
      ILogger<GameChannelInitializer> logger
      )
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      var expansion = _options.WoW.GetExpansion();

      _gamePacketHandler = gamePacketHandler ?? throw new ArgumentNullException(nameof(gamePacketHandler));
      _gamePacketDecoder = gamePacketDecoderResolver(expansion) ?? throw new ArgumentNullException(nameof(gamePacketDecoderResolver));
      _gamePacketEncoder = gamePacketEncoderResolver(expansion) ?? throw new ArgumentNullException(nameof(gamePacketEncoderResolver));
      _idleStateCallback = idleStateCallback ?? throw new ArgumentNullException(nameof(idleStateCallback));

      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public GamePacketHandler GamePacketHandler { get { return _gamePacketHandler; } }

    public void SetConnectionOptions(GameServerInfo realm, SessionInfo session)
    {
      _gamePacketHandler.Realm = realm;
      _gamePacketHandler.Session = session;
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