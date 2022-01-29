namespace WoWChat.Net.Game
{
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;

  public class GamePacketHandlerWotLK : GamePacketHandlerTBC
  {
    public GamePacketHandlerWotLK(IOptionsSnapshot<WowChatOptions> options, ILogger<GamePacketHandler> logger)
      : base(options, logger)
    {
    }
  }
}
