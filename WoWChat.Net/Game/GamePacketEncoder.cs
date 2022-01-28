namespace WoWChat.Net.Game;

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using global::WoWChat.Net.Common;
using global::WoWChat.Net.Extensions;
using Microsoft.Extensions.Logging;

public class GamePacketEncoder : MessageToByteEncoder<Packet>
{
  private readonly ILogger<GamePacketEncoder> _logger;

  public GamePacketEncoder(ILogger<GamePacketEncoder> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  protected override void Encode(IChannelHandlerContext context, Packet message, IByteBuffer output)
  {
    _logger.LogDebug("SEND GAME PACKET: {id} - {byteBuf}", message.Id, BitConverter.ToString(message.ByteBuf.GetArrayCopy()));

    output.WriteByte(message.Id);
    output.WriteBytes(message.ByteBuf);
    message.ByteBuf.Release();
  }

  protected bool IsUnencryptedPacket(int id){
    return id == WorldCommand.CMSG_AUTH_CHALLENGE;
  }
}
