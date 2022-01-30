namespace WoWChat.Net.Game;

using Microsoft.Extensions.Logging;

public class GamePacketEncoderCataclysm : GamePacketEncoder
{
  protected static int WOW_CONNECTION = 0x4F57; // same hack as in mangos :D

  public GamePacketEncoderCataclysm(GameHeaderCryptWotLK crypt, ILogger<GamePacketEncoderCataclysm> logger)
    : base(crypt, logger)
  {
  }

  protected override bool IsUnencryptedPacket(int id)
  {
    return base.IsUnencryptedPacket(id) || id == WOW_CONNECTION;
  }
}
