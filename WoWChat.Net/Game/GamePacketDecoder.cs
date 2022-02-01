namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Extensions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Generic;

  public class GamePacketDecoder : ByteToMessageDecoder
  {
    protected readonly GameHeaderCrypt _crypt;
    private readonly ILogger<GamePacketDecoder> _logger;

    protected int HEADER_LENGTH = 4;

    private int _size = 0;
    private int _id = 0;

    public GamePacketDecoder(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketDecoder> logger)
    {
      var localOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _crypt = cryptResolver(localOptions.WoW.GetExpansion()) ?? throw new ArgumentNullException(nameof(cryptResolver));
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
        // decrypt if necessary
        var tuple = _crypt.IsInitialized
          ? ParseGameHeaderEncrypted(input)
          : ParseGameHeader(input);

        _id = tuple.Item1;
        _size = tuple.Item2;
      }

      if (_size > input.ReadableBytes)
      {
        // Wait for next recv
        return;
      }

      var byteBuf = input.ReadBytes(_size);

      // decompress if necessary
      var (newId, decompressed) = Decompress(context, _id, byteBuf);


      var packet = new Packet(newId, decompressed);

      if (_logger.IsEnabled(LogLevel.Debug))
      {
        _logger.LogDebug("RECV GAME PACKET: {id} - {byteBuf}", BitConverter.ToString(newId.ToBytes()), BitConverter.ToString(decompressed.GetArrayCopy()));
      }

      output.Add(packet);

      // As we sucessfully read a packet, reset id and size to set up for the next packet.
      _id = 0;
      _size = 0;
    }

    protected virtual (int, int) ParseGameHeader(IByteBuffer input)
    {
      var size = input.ReadShort() - 2;
      var id = input.ReadShortLE();
      return (id, size);
    }

    protected virtual (int, int) ParseGameHeaderEncrypted(IByteBuffer input)
    {
      var header = new byte[HEADER_LENGTH];
      input.ReadBytes(header);
      var decrypted = _crypt.Decrypt(header);
      var size = ((decrypted[0] & 0xFF) << 8 | decrypted[1] & 0xFF) - 2;
      var id = (decrypted[3] & 0xFF) << 8 | decrypted[2] & 0xFF;
      return (id, size);
    }

    /// <summary>
    /// Decompresses the specified byte buffer
    /// </summary>
    /// <remarks>
    /// vanilla has no compression. starts in cata/mop
    /// </remarks>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual (int, IByteBuffer) Decompress(IChannelHandlerContext context, int id, IByteBuffer input)
    {
      return (id, input);
    }
  }
}
