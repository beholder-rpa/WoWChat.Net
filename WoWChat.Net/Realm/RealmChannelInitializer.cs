namespace WoWChat.Net.Realm
{
  using Common;
  using DotNetty.Handlers.Timeout;
  using DotNetty.Transport.Channels;
  using DotNetty.Transport.Channels.Sockets;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;

  public class RealmChannelInitializer : ChannelInitializer<TcpSocketChannel>
  {
    private readonly RealmPacketHandler _realmPacketHandler;
    private readonly RealmPacketDecoder _realmPacketDecoder;
    private readonly RealmPacketEncoder _realmPacketEncoder;
    private readonly IdleStateCallback _idleStateCallback;

    protected readonly WowChatOptions _options;
    protected readonly ILogger<RealmChannelInitializer> _logger;

    public RealmChannelInitializer(
      IOptionsSnapshot<WowChatOptions> options,
      RealmPacketHandlerResolver realmPacketHandlerResolver,
      RealmPacketDecoder realmPacketDecoder,
      RealmPacketEncoder realmPacketEncoder,
      IdleStateCallback idleStateCallback,

      ILogger<RealmChannelInitializer> logger
      )
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _realmPacketHandler = realmPacketHandlerResolver(_options.GetExpansion()) ?? throw new NotImplementedException($"Unable to locate a realm packet handler for expansion {_options.GetExpansion()}");
      _realmPacketDecoder = realmPacketDecoder ?? throw new ArgumentNullException(nameof(realmPacketDecoder));
      _realmPacketEncoder = realmPacketEncoder ?? throw new ArgumentNullException(nameof(realmPacketEncoder));
      _idleStateCallback = idleStateCallback ?? throw new ArgumentNullException(nameof(idleStateCallback));
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
