namespace WoWChat.Net.Realm.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using global::WoWChat.Net.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Text.RegularExpressions;

[PacketHandler(RealmCommand.CMD_REALM_LIST, WoWExpansion.Vanilla)]
public class RealmListPacketHandler : IPacketHandler<RealmEvent>
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<RealmListPacketHandler> _logger;

  public RealmListPacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<RealmListPacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<RealmEvent>? EventCallback { get; set; }

  public bool IsExpectedDisconnect { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var realmList = ParseRealmList(msg);
    EventCallback?.Invoke(new RealmListEvent()
    {
      RealmList = realmList
    });

    IsExpectedDisconnect = true;
    ctx.CloseAsync().Wait();
    EventCallback?.Invoke(new RealmDisconnectedEvent()
    {
      AutoReconnect = false,
      IsExpected = true,
      Reason = "Realm List Retrieved"
    });
  }

  protected virtual IList<GameRealm> ParseRealmList(Packet msg)
  {
    var result = new List<GameRealm>();
    msg.ByteBuf.ReadIntLE(); // unknown
    var numRealms = msg.ByteBuf.ReadByte();
    for (int i = 0; i < numRealms; i++)
    {
      var realmType = msg.ByteBuf.ReadByte(); // realm type (pvp/pve)
      var realmLocked = msg.ByteBuf.ReadByte(); // Locked/Unlocked
      var realmFlags = msg.ByteBuf.ReadByte(); // realm flags (offline/recommended/for newbs)

      // On Vanilla MaNGOS, there is some string manipulation to insert the build information into the name itself
      // if realm flags specify to do so. But that is counter-intuitive to matching the config, so let's remove it.
      var name = (realmFlags & 0x04) == 0x04
        ? Regex.Replace(msg.ByteBuf.ReadString(), " \\(\\d+,\\d+,\\d+\\)", "")
        : msg.ByteBuf.ReadString();

      var address = msg.ByteBuf.ReadString();
      var population = msg.ByteBuf.ReadUnsignedInt(); // population
      var characters = msg.ByteBuf.ReadByte(); // num of characters
      var timeZone = msg.ByteBuf.ReadByte(); // timezone
      var realmId = msg.ByteBuf.ReadByte();

      var addressTokens = address.Split(':');
      var host = addressTokens[0];
      // some servers "overflow" the port on purpose to dissuade rudimentary bots
      var port = addressTokens.Length > 1 ? int.Parse(addressTokens[1]) & 0xFFFF : 8085;

      result.Add(new GameRealm()
      {
        Type = realmType,
        Locked = realmLocked,
        Flags = realmFlags,
        Name = name,
        Host = host,
        Port = port,
        Population = population,
        Characters = characters,
        TimeZone = timeZone,
        RealmId = realmId,
      }); ;
    }

    return result;
  }
}
