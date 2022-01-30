namespace WoWChat.Net.Game;

using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Extensions;
using Microsoft.Extensions.Logging;

public class GamePacketEncoderMoP : GamePacketEncoder
{
  public GamePacketEncoderMoP(GameHeaderCryptMoP crypt, ILogger<GamePacketEncoderMoP> logger)
    : base(crypt, logger)
  {
  }

  protected override void Encode(IChannelHandlerContext context, Common.Packet message, IByteBuffer output)
  {
    var isUnencrypted = IsUnencryptedPacket(message.Id);

    var headerSize = 4;
    var size = message.ByteBuf.WriterIndex;

    using var ms = new MemoryStream(headerSize);
    byte[] header;
    if (isUnencrypted)
    {
      //FIXME: These might need to be switched to Little Endian
      ms.Write(BitConverter.GetBytes(size + 2));
      ms.Write(BitConverter.GetBytes(message.Id));
      header = ms.ToArray();
    }
    else
    {
      //FIXME: These might need to be switched to Little Endian
      ms.Write(BitConverter.GetBytes((size << 13) | (message.Id & 0x1FFF)));
      header = _crypt.Encrypt(ms.ToArray());
    }

    _logger.LogDebug("SEND GAME PACKET: {id} - {byteBuf}", message.Id, BitConverter.ToString(message.ByteBuf.GetArrayCopy()));

    output.WriteBytes(header);
    output.WriteBytes(message.ByteBuf);
    message.ByteBuf.Release();
  }
}
