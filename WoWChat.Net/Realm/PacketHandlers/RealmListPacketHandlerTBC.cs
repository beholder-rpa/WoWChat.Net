namespace WoWChat.Net.Realm.PacketHandlers;

using Common;
using global::WoWChat.Net.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(RealmCommand.CMD_REALM_LIST, WoWExpansion.TBC | WoWExpansion.WotLK | WoWExpansion.Cataclysm | WoWExpansion.MoP)]
public class RealmListPacketHandlerTBC : RealmListPacketHandler
{
  public RealmListPacketHandlerTBC(IOptionsSnapshot<WowChatOptions> options, ILogger<RealmListPacketHandler> logger) : base(options, logger)
  {
  }

  protected override IList<GameServerInfo> ParseRealmList(Packet msg)
  {
    var result = new List<GameServerInfo>();
    msg.ByteBuf.ReadIntLE(); // unknown
    var numRealms = msg.ByteBuf.ReadByte();
    for (int i = 0; i < numRealms; i++)
    {
      var realmType = msg.ByteBuf.ReadByte(); // realm type (pvp/pve)
      var realmLocked = msg.ByteBuf.ReadByte(); // Locked/Unlocked
      var realmFlags = msg.ByteBuf.ReadByte(); // realm flags (offline/recommended/for newbs)
      var name = msg.ByteBuf.ReadString();
      var address = msg.ByteBuf.ReadString();
      var population = msg.ByteBuf.ReadUnsignedInt(); // population
      var characters = msg.ByteBuf.ReadByte(); // num of characters
      var timeZone = msg.ByteBuf.ReadByte(); // timezone
      var realmId = msg.ByteBuf.ReadByte();

      var addressTokens = address.Split(':');
      var host = addressTokens[0];
      var port = addressTokens.Length > 1 ? int.Parse(addressTokens[1]) : 8085;

      var realmInfo = new GameServerInfo()
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
      };

      // BC/wotlk include build information in the packet
      if ((realmFlags & 0x04) != 0)
      {
        var versionMajor = msg.ByteBuf.ReadByte();
        var versionMinor = msg.ByteBuf.ReadByte();
        var versionBugfix = msg.ByteBuf.ReadByte();
        var build = msg.ByteBuf.ReadUnsignedShort();

        realmInfo = realmInfo with { Version = new Version(versionMajor, versionMinor, versionBugfix, build) };
      }

      result.Add(realmInfo);
    }

    return result;
  }
}