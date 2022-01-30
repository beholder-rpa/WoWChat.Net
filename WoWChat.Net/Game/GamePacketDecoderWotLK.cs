namespace WoWChat.Net.Game
{
  using DotNetty.Buffers;
  using Options;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;

  public class GamePacketDecoderWotLK : GamePacketDecoder
  {
    public GamePacketDecoderWotLK(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketDecoderWotLK> logger)
      : base(options, cryptResolver, logger)
    {
    }

    protected override (int, int) ParseGameHeaderEncrypted(IByteBuffer input)
    {
      var header = new byte[HEADER_LENGTH];
      input.ReadBytes(header);
      var decrypted = _crypt.Decrypt(header);

      // WotLK and later expansions have a variable size header. An extra byte is included if the size is > 0x7FFF
      if ((decrypted[0] & 0x80) == 0x80)
      {
        var nextByte = _crypt.Decrypt(new byte[] { input.ReadByte() })[0];
        var size = (((decrypted[0] & 0x7F) << 16) | ((decrypted[1] & 0xFF) << 8) | (decrypted[2] & 0xFF)) - 2;
        var id = (nextByte & 0xFF) << 8 | decrypted[3] & 0xFF;
        return (id, size);
      }
      else
      {
        var size = ((decrypted[0] & 0xFF) << 8 | decrypted[1] & 0xFF) - 2;
        var id = (decrypted[3] & 0xFF) << 8 | decrypted[2] & 0xFF;
        return (id, size);
      }
    }
  }
}
