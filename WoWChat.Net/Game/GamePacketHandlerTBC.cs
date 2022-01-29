namespace WoWChat.Net.Game
{
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;

  public class GamePacketHandlerTBC : GamePacketHandler
  {
    public GamePacketHandlerTBC(IOptionsSnapshot<WowChatOptions> options, ILogger<GamePacketHandler> logger)
      : base(options, logger)
    {
    }
  }
}
