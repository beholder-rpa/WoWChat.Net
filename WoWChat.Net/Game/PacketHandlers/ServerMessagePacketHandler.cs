namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(WorldCommand.SMSG_SERVER_MESSAGE, WoWExpansion.Vanilla | WoWExpansion.TBC | WoWExpansion.WotLK)]
//[PacketHandler(WorldCommandCataclysm.SMSG_SERVER_MESSAGE, WoWExpansion.Cataclysm)]
//[PacketHandler(WorldCommandMoP.SMSG_SERVER_MESSAGE, WoWExpansion.MoP)]
public class ServerMessagePacketHandler : IPacketHandler<GameEvent>
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<ServerChatMessagePacketHandler> _logger;

  public ServerMessagePacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<ServerChatMessagePacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var tp = msg.ByteBuf.ReadIntLE();
    var txt = msg.ByteBuf.ReadString();

    EventCallback?.Invoke(new GameServerMessageEvent()
    {
      Kind = (ServerMessageKind)tp,
      Message = txt
    });
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