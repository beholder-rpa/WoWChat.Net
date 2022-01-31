namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_INVALIDATE_PLAYER, WoWExpansion.Vanilla | WoWExpansion.TBC | WoWExpansion.WotLK)]
public class ServerInvalidatePlayerHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerInvalidatePlayerHandler> _logger;

  public ServerInvalidatePlayerHandler(ILogger<ServerInvalidatePlayerHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var guid = msg.ByteBuf.ReadLongLE();
    _logger.LogDebug("Invalidate Player: {guid}", guid);
  }
}
