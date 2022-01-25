namespace WoWChat.Net.Realm
{
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Generic;
  using Common;

  public class RealmPacketDecoder : ByteToMessageDecoder
  {
    private readonly ILogger<RealmPacketDecoder> _logger;

    private int _size = 0;
    private int _id = 0;

    public RealmPacketDecoder(ILogger<RealmPacketDecoder> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
      if (input.ReadableBytes == 0)
      {
        return;
      }

      if (_size == 0 && _id == 0)
      {

      }

      if (_size > input.ReadableBytes)
      {
        return;
      }

      var byteBuf = input.ReadBytes(_size);
      var packet = new Packet(_id, byteBuf.Array);

      _logger.LogDebug($"RECV REALM PACKET: {_id} - {byteBuf:X2}");

      output.Add(packet);
    }
  }
}
