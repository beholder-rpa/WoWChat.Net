namespace WoWChat.Net.Game
{
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;

  public class GamePacketHandlerWotLK : GamePacketHandlerTBC
  {
    public GamePacketHandlerWotLK(IServiceProvider serviceProvider, IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver headerCryptResolver, ILogger<GamePacketHandler> logger)
      : base(serviceProvider, options, headerCryptResolver, logger)
    {
    }

  }
}
