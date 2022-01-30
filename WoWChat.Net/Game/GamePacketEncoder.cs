namespace WoWChat.Net.Game;

using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Extensions;
using Microsoft.Extensions.Logging;

public class GamePacketEncoder : MessageToByteEncoder<Packet>
{
  protected readonly GameHeaderCrypt _crypt;
  protected readonly ILogger<GamePacketEncoder> _logger;

  public GamePacketEncoder(GameHeaderCrypt crypt, ILogger<GamePacketEncoder> logger)
  {
    _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  protected override void Encode(IChannelHandlerContext context, Packet message, IByteBuffer output)
  {
    var isUnencrypted = IsUnencryptedPacket(message.Id);

    var headerSize = isUnencrypted ? 4 : 6;

    using var ms = new MemoryStream(headerSize);
    ms.Write((message.ByteBuf.WriterIndex + headerSize - 2).ToBytesShort());
    ms.Write(message.Id.ToBytesShort());

    byte[] header;
    if (isUnencrypted)
    {
      header = ms.ToArray();
    }
    else
    {
      ms.WriteByte(0);
      ms.WriteByte(0);
      header = _crypt.Encrypt(ms.ToArray());
    }

    _logger.LogDebug("SEND GAME PACKET: {id} - {byteBuf}", BitConverter.ToString(message.Id.ToBytes()), BitConverter.ToString(message.ByteBuf.GetArrayCopy()));

    output.WriteBytes(header);
    output.WriteBytes(message.ByteBuf);
    message.ByteBuf.Release();
  }

  protected virtual bool IsUnencryptedPacket(int id)
  {
    return id == WorldCommand.CMSG_AUTH_CHALLENGE;
  }
}
