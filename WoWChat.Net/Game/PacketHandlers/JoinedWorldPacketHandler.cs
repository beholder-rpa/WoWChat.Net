namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_LOGIN_VERIFY_WORLD, WoWExpansion.All)]
public class JoinedWorldPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<JoinedWorldPacketHandler> _logger;

  public JoinedWorldPacketHandler(ILogger<JoinedWorldPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    EventCallback?.Invoke(new GameJoinedWorldEvent());
    _logger.LogDebug("SMSG_LOGIN_VERIFY_WORLD");
  }
}
