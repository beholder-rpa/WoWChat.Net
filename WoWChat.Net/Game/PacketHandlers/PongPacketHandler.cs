namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_PONG, WoWExpansion.All)]
public class PongPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<PongPacketHandler> _logger;

  public PongPacketHandler(ILogger<PongPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var pingId = msg.ByteBuf.ReadIntLE();
    _logger.LogDebug("PONG: {pingId}", pingId);
  }
}