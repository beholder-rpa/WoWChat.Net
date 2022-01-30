namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(WorldCommand.SMSG_SERVER_MESSAGE, WoWExpansion.Vanilla)]
[PacketHandler(WorldCommand.SMSG_SERVER_MESSAGE, WoWExpansion.TBC)]
[PacketHandler(WorldCommand.SMSG_SERVER_MESSAGE, WoWExpansion.WotLK)]
//[PacketHandler(WorldCommandCataclysm.SMSG_SERVER_MESSAGE, WoWExpansion.Cataclysm)]
//[PacketHandler(WorldCommandMoP.SMSG_SERVER_MESSAGE, WoWExpansion.MoP)]
public class ServerMessagePacketHandler : IPacketHandler
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<ServerMessagePacketHandler> _logger;

  public ServerMessagePacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<ServerMessagePacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? GameEventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    //TODO...
  }

  protected enum ServerMessageType : byte
  {
    SERVER_MSG_SHUTDOWN_TIME = 0x01,
    SERVER_MSG_RESTART_TIME = 0x02,
    SERVER_MSG_CUSTOM = 0x03,
    SERVER_MSG_SHUTDOWN_CANCELLED = 0x04,
    SERVER_MSG_RESTART_CANCELLED = 0x05,
  }
}
