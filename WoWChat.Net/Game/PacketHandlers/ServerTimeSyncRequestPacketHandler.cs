namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommandTBC.SMSG_TIME_SYNC_REQ, WoWExpansion.TBC | WoWExpansion.WotLK | WoWExpansion.Cataclysm | WoWExpansion.MoP)]
public class ServerTimeSyncRequestPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerTimeSyncRequestPacketHandler> _logger;
  protected readonly int _connectTime;

  public ServerTimeSyncRequestPacketHandler(ILogger<ServerTimeSyncRequestPacketHandler> logger)
  {
    _connectTime = Environment.TickCount;
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var counter = msg.ByteBuf.ReadIntLE();
    var uptime = Environment.TickCount - _connectTime;

    var byteBuf = ctx.Allocator.Buffer(8, 8);
    byteBuf.WriteIntLE(counter);
    byteBuf.WriteIntLE(uptime);
    ctx.WriteAndFlushAsync(new Packet(WorldCommandTBC.CMSG_TIME_SYNC_RESP, byteBuf));
  }
}