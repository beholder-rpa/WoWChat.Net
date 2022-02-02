namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_JOIN_CHANNEL, WoWExpansion.TBC | WoWExpansion.WotLK)]
public class JoinChannelCommandTBC : JoinChannelCommand
{
  public JoinChannelCommandTBC(GameChannelLookup channelLookup, ILogger<KeepAliveCommand> logger)
    : base(channelLookup, logger)
  {
  }

  protected override void WriteJoinChannel(IByteBuffer output, int id, string channelName)
  {
    output.WriteIntLE(id);
    output.WriteByte(0);
    output.WriteByte(1);
    base.WriteJoinChannel(output, id, channelName);
  }
}