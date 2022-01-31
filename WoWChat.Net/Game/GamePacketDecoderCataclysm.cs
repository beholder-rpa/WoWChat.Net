namespace WoWChat.Net.Game
{
  using DotNetty.Buffers;
  using DotNetty.Transport.Channels;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System.IO.Compression;

  public class GamePacketDecoderCataclysm : GamePacketDecoderWotLK
  {
    protected static int COMPRESSED_DATA_MASK = 0x8000;

    public GamePacketDecoderCataclysm(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketDecoderWotLK> logger)
      : base(options, cryptResolver, logger)
    {
    }

    protected override (int, IByteBuffer) Decompress(IChannelHandlerContext context, int id, IByteBuffer input)
    {
      if (IsCompressed(id))
      {
        var decompressedSize = GetDecompressedSize(input);

        var compressed = new byte[input.ReadableBytes];
        input.ReadBytes(compressed);
        input.Release();

        using var decompressedStream = new MemoryStream(decompressedSize);
        using var compressedStream = new MemoryStream(compressed);
        using var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress);
        decompressor.CopyTo(decompressedStream);
        var decompressed = decompressedStream.ToArray();

        var ret = context.Allocator.Buffer(decompressed.Length, decompressed.Length);
        ret.WriteBytes(decompressed);
        return (GetDecompressedId(id, ret), ret);
      }
      else
      {
        return (id, input);
      }
    }

    protected virtual int GetDecompressedSize(IByteBuffer byteBuf)
    {
      return byteBuf.ReadIntLE();
    }

    protected virtual int GetDecompressedId(int id, IByteBuffer byteBuf)
    {
      return id ^ COMPRESSED_DATA_MASK;
    }

    protected virtual bool IsCompressed(int id)
    {
      return (id & COMPRESSED_DATA_MASK) == COMPRESSED_DATA_MASK;
    }
  }
}
