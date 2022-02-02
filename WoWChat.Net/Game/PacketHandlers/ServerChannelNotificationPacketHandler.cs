namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_CHANNEL_NOTIFY, WoWExpansion.All)]
public class ServerChannelNotificationPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerChannelNotificationPacketHandler> _logger;

  public ServerChannelNotificationPacketHandler(ILogger<ServerChannelNotificationPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var kind = msg.ByteBuf.ReadByte();
    var channelName = msg.ByteBuf.ReadString();

    EventCallback?.Invoke(new GameChannelNotificationEvent()
    {
      Kind = (ChatNotificationKind)kind,
      ChannelName = channelName,
    });

    _logger.LogDebug("SMSG_CHANNEL_NOTIFY - {kind} {channelName}", kind, channelName);
  }
}