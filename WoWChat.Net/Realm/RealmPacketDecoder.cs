namespace WoWChat.Net.Realm
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

  public class RealmPacketDecoder : ByteToMessageDecoder
  {
    private readonly WowChatOptions _options;
    private readonly ILogger<RealmPacketDecoder> _logger;

    private int _size = 0;
    private byte _id = 0;

    public RealmPacketDecoder(IOptions<WowChatOptions> options, ILogger<RealmPacketDecoder> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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
        input.MarkReaderIndex();
        _id = input.ReadByte();

        switch (_id)
        {
          case (byte)RealmAuthCommand.CMD_AUTH_LOGON_CHALLENGE:
            if (input.ReadableBytes < 2)
            {
              input.ResetReaderIndex();
              return;
            }
            input.MarkReaderIndex();
            input.SkipBytes(1);
            var result = input.ReadByte();
            if (RealmHelpers.IsAuthResultSuccess(result))
            {
              _size = 118;
            }
            else
            {
              _size = 2;
            }
            input.ResetReaderIndex();
            break;
          case (byte)RealmAuthCommand.CMD_AUTH_LOGON_PROOF:
            if (input.ReadableBytes < 1)
            {
              input.ResetReaderIndex();
              return;
            }

            // size is error dependent
            input.MarkReaderIndex();
            var result1 = input.ReadByte();
            if ((byte)RealmAuthResult.WOW_SUCCESS == result1)
            {
              _size = (_options.GetExpansion() == WoWExpansion.Vanilla) ? 25 : 31;
            }
            else
            {
              // A failure authentication result should be 1 byte in length for vanilla and 3 bytes for other expansions.
              // Some servers send back a malformed 1 byte response even for later expansions.
              _size = input.ReadableBytes == 0 ? 1 : 3;
            }
            input.ResetReaderIndex();
            break;
          case (byte)RealmAuthCommand.CMD_REALM_LIST:
            if (input.ReadableBytes < 2)
            {
              input.ResetReaderIndex();
              return;
            }
            _size = input.ReadShortLE();
            break;
        }
      }

      if (_size > input.ReadableBytes)
      {
        // Wait for next recv
        return;
      }

      var byteBuf = input.ReadBytes(_size);
      var packet = new Packet(_id, byteBuf);

      _logger.LogDebug("RECV REALM PACKET: {id} - {byteBuf}", _id, BitConverter.ToString(byteBuf.GetArrayCopy()));

      output.Add(packet);

      // As we sucessfully read a packet, reset id and size to set up for the next packet.
      _id = 0;
      _size = 0;
    }
  }
}
