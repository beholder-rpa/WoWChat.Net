namespace WoWChat.Net.Realm
{
  using Common;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Extensions;
  using Microsoft.Extensions.Logging;
  using System;

  public class RealmPacketEncoder : MessageToByteEncoder<Packet>
  {
    private readonly ILogger<RealmPacketEncoder> _logger;

    public RealmPacketEncoder(ILogger<RealmPacketEncoder> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void Encode(IChannelHandlerContext context, Packet message, IByteBuffer output)
    {
      _logger.LogDebug("SEND REALM PACKET: {id} - {byteBuf}", BitConverter.ToString(message.Id.ToBytes()), BitConverter.ToString(message.ByteBuf.GetArrayCopy()));

      output.WriteByte(message.Id);
      output.WriteBytes(message.ByteBuf);
      message.ByteBuf.Release();
    }
  }
}
