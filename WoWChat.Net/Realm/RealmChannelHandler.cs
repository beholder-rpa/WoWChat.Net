namespace WoWChat.Net.Realm
{
  using DotNetty.Handlers.Timeout;
  using DotNetty.Transport.Channels;
  using DotNetty.Transport.Channels.Sockets;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using System;
  using Common;
  using Options;

  public class RealmChannelHandler : ChannelInitializer<TcpSocketChannel>
  {
    private readonly RealmPacketHandler _realmPacketHandler;
    private readonly RealmPacketDecoder _realmPacketDecoder;
    private readonly RealmPacketEncoder _realmPacketEncoder;
    private readonly IdleStateCallback _idleStateCallback;

    protected readonly WowChatOptions _options;
    protected readonly ILogger<RealmChannelHandler> _logger;

    public RealmChannelHandler(
      RealmPacketHandler realmPacketHandler,
      RealmPacketDecoder realmPacketDecoder,
      RealmPacketEncoder realmPacketEncoder,
      IdleStateCallback idleStateCallback,
      IOptions<WowChatOptions> options,
      ILogger<RealmChannelHandler> logger
      )
    {
      _realmPacketHandler = realmPacketHandler ?? throw new ArgumentNullException(nameof(realmPacketHandler));
      _realmPacketDecoder = realmPacketDecoder ?? throw new ArgumentNullException(nameof(realmPacketDecoder));
      _realmPacketEncoder = realmPacketEncoder ?? throw new ArgumentNullException(nameof(realmPacketEncoder));
      _idleStateCallback = idleStateCallback ?? throw new ArgumentNullException(nameof(idleStateCallback));
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void InitChannel(TcpSocketChannel channel)
    {
      channel.Pipeline.AddLast(
        new IdleStateHandler(60, 120, 0),
        _idleStateCallback,
        _realmPacketDecoder,
        _realmPacketEncoder,
        _realmPacketHandler
      );
    }
  }
}
