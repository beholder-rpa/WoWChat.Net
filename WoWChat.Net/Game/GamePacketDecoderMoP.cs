namespace WoWChat.Net.Game
{
  using DotNetty.Buffers;
  using Options;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;

  public class GamePacketDecoderMoP : GamePacketDecoderCataclysm
  {
    protected static int SMSG_COMPRESSED_DATA = 0x1568;

    public GamePacketDecoderMoP(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketDecoderMoP> logger)
      : base(options, cryptResolver, logger)
    {
    }

    protected override (int, int) ParseGameHeader(IByteBuffer input) {
      var size = input.ReadShortLE() - 2;
      var id = input.ReadShortLE();
      return (id, size);
    }

    protected override (int, int) ParseGameHeaderEncrypted(IByteBuffer input) {
      var header = new byte[HEADER_LENGTH];
      input.ReadBytes(header);
      var decrypted = _crypt.Decrypt(header);
      // FIXME: This might need to be Little Endian
      var raw = (int)BitConverter.ToUInt64(decrypted);
      var id = raw & 0x1FFF;
      var size = raw >> 13;
      return (id, size);
    }

    protected override int GetDecompressedSize(IByteBuffer byteBuf) {
      var size = byteBuf.ReadIntLE();
      byteBuf.SkipBytes(8); // skip adler checksums
      return size;
    }

    protected override int GetDecompressedId(int id, IByteBuffer byteBuf)
    {
      var newId = byteBuf.ReadShortLE();
      byteBuf.SkipBytes(2);
      return newId;
    }

    protected override bool IsCompressed(int id)
    {
      return id == SMSG_COMPRESSED_DATA;
    }
  }
}
