namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommandWotLK.SMSG_ZONE_UNDER_ATTACK, WoWExpansion.WotLK)]
public class ServerZoneUnderAttackPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerZoneUnderAttackPacketHandler> _logger;

  public ServerZoneUnderAttackPacketHandler(ILogger<ServerZoneUnderAttackPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var zoneId = msg.ByteBuf.ReadIntLE();
    EventCallback?.Invoke(new GameZoneUnderAttackEvent()
    {
      ZoneId = zoneId,
    });
    _logger.LogDebug("SMSG_ZONE_UNDER_ATTACK: {zoneId}", zoneId);
  }
}