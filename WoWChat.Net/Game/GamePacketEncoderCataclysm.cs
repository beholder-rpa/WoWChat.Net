namespace WoWChat.Net.Game;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

public class GamePacketEncoderCataclysm : GamePacketEncoder
{
  protected static int WOW_CONNECTION = 0x4F57; // same hack as in mangos :D

  public GamePacketEncoderCataclysm(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver cryptResolver, ILogger<GamePacketEncoderCataclysm> logger)
    : base(options, cryptResolver, logger)
  {
  }

  protected override bool IsUnencryptedPacket(int id)
  {
    return base.IsUnencryptedPacket(id) || id == WOW_CONNECTION;
  }
}
