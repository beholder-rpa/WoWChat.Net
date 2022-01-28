namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Extensions;
  using Helpers;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Generic;

  public class GamePacketDecoder : ByteToMessageDecoder
  {
    private readonly WowChatOptions _options;
    private readonly ILogger<GamePacketDecoder> _logger;

    protected int HEADER_LENGTH = 4;

    private int _size = 0;
    private int _id = 0;

    public GamePacketDecoder(IOptions<WowChatOptions> options, ILogger<GamePacketDecoder> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
      if (input.ReadableBytes < HEADER_LENGTH)
      {
        return;
      }

      if (_size == 0 && _id == 0)
      {
        
      }

      if (_size > input.ReadableBytes)
      {
        // Wait for next recv
        return;
      }

      var byteBuf = input.ReadBytes(_size);

      // decompress if necessary
      var (newId, decompressed) = Decompress(_id, byteBuf);


      var packet = new Packet(_id, byteBuf);

      _logger.LogDebug("RECV REALM PACKET: {id} - {byteBuf}", _id, BitConverter.ToString(byteBuf.GetArrayCopy()));

      output.Add(packet);

      // As we sucessfully read a packet, reset id and size to set up for the next packet.
      _id = 0;
      _size = 0;
    }

    /// <summary>
    /// Decompresses the specified byte buffer
    /// </summary>
    /// <remarks>
    /// vanilla has no compression. starts in cata/mop
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual (int, IByteBuffer) Decompress(int id, IByteBuffer input) {
      return (id, input);
    }
  }
}
