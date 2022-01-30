namespace WoWChat.Net.Game;

using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Extensions;
using global::WoWChat.Net.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GamePacketEncoderMoP : GamePacketEncoder
{
  public GamePacketEncoderMoP(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketEncoderMoP> logger)
    : base(options, cryptResolver, logger)
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
      ms.Write((message.ByteBuf.WriterIndex + headerSize - 2).ToBytesShortLE());
      ms.Write(message.Id.ToBytesShortLE());
      ms.Write(BitConverter.GetBytes(size + 2));
      ms.Write(BitConverter.GetBytes(message.Id));
      header = ms.ToArray();
    }
    else
    {
      ms.Write((size << 13 | (message.Id & 0x1FFF)).ToBytesLE());
      header = _crypt.Encrypt(ms.ToArray());
    }

    _logger.LogDebug("SEND GAME PACKET HEADER: {header}", BitConverter.ToString(header));
    _logger.LogDebug("SEND GAME PACKET: {id} - {byteBuf}", message.Id, BitConverter.ToString(message.ByteBuf.GetArrayCopy()));

    output.WriteBytes(header);
    output.WriteBytes(message.ByteBuf);
    message.ByteBuf.Release();
  }
}
