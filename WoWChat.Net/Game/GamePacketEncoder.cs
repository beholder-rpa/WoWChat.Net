namespace WoWChat.Net.Game;

using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

public class GamePacketEncoder : MessageToByteEncoder<Packet>
{
  protected readonly GameHeaderCrypt _crypt;
  protected readonly ILogger<GamePacketEncoder> _logger;

  public GamePacketEncoder(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketEncoder> logger)
  {
    var localOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _crypt = cryptResolver(localOptions.WoW.GetExpansion());
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  protected override void Encode(IChannelHandlerContext context, Packet message, IByteBuffer output)
  {
    var isUnencrypted = IsUnencryptedPacket(message.Id);

    var headerSize = isUnencrypted ? 4 : 6;

    using var ms = new MemoryStream(headerSize);
    ms.Write((message.ByteBuf.WriterIndex + headerSize - 2).ToBytesShort());
    ms.Write(message.Id.ToBytesShortLE());

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

    if (_logger.IsEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("SEND GAME PACKET HEADER: {header}", BitConverter.ToString(header));
      _logger.LogDebug("SEND GAME PACKET: {id} - {byteBuf}", BitConverter.ToString(message.Id.ToBytes()), BitConverter.ToString(message.ByteBuf.GetArrayCopy()));
    }

    output.WriteBytes(header);
    output.WriteBytes(message.ByteBuf);
    message.ByteBuf.Release();
  }

  protected virtual bool IsUnencryptedPacket(int id)
  {
    return id == WorldCommand.CMSG_AUTH_CHALLENGE;
  }
}