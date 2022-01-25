namespace WoWChat.Net.Realm
{
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;

  public class RealmPacketHandlerTBC : RealmPacketHandler
  {
    public RealmPacketHandlerTBC(IOptions<WowChatOptions> options, ILogger<RealmPacketHandler> logger) : base(options, logger)
    {
    }
  }
}
