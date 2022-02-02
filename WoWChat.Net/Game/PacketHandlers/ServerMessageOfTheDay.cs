namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;
using System.Text;

[PacketHandler(WorldCommandTBC.SMSG_MOTD, WoWExpansion.TBC | WoWExpansion.WotLK)]
public class ServerMessageOfTheDayHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerMessageOfTheDayHandler> _logger;

  public ServerMessageOfTheDayHandler(ILogger<ServerMessageOfTheDayHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var lineCount = msg.ByteBuf.ReadIntLE();
    var lines = new StringBuilder();
    for(int i = 0; i < lineCount; i++)
    {
      var message = msg.ByteBuf.ReadString();
      lines.AppendLine(message);
    }
    EventCallback?.Invoke(new GameJoinedWorldEvent());
    _logger.LogDebug("SMSG_MOTD: {lineCount}", lineCount);
  }
}