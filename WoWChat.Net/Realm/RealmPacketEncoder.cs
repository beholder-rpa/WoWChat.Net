namespace WoWChat.Net.Realm
{
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Microsoft.Extensions.Logging;
  using System;
  using Common;

  public class RealmPacketEncoder : MessageToByteEncoder<Packet>
  {
    private readonly ILogger<RealmPacketEncoder> _logger;

    public RealmPacketEncoder(ILogger<RealmPacketEncoder> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void Encode(IChannelHandlerContext context, Packet message, IByteBuffer output)
    {
      _logger.LogDebug($"SEND REALM PACKET: {message.CommandId} - {message.ByteBuf:X2}");

      output.WriteByte(message.CommandId);
      output.WriteBytes(message.ByteBuf);
      message.Dispose();
    }
  }
}
