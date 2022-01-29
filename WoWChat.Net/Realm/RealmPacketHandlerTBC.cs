namespace WoWChat.Net.Realm
{
  using Common;
  using Extensions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System.Collections.Generic;

  public class RealmPacketHandlerTBC : RealmPacketHandler
  {
    public RealmPacketHandlerTBC(IOptionsSnapshot<WowChatOptions> options, ILogger<RealmPacketHandlerTBC> logger)
      : base(options, logger)
    {
    }

    protected override IList<Realm> ParseRealmList(Packet packet)
    {
      var result = new List<Realm>();
      packet.ByteBuf.ReadIntLE(); // unknown
      var numRealms = packet.ByteBuf.ReadByte();
      for (int i = 0; i < numRealms; i++)
      {
        var realmType = packet.ByteBuf.ReadByte(); // realm type (pvp/pve)
        var realmLocked = packet.ByteBuf.ReadByte(); // Locked/Unlocked
        var realmFlags = packet.ByteBuf.ReadByte(); // realm flags (offline/recommended/for newbs)
        var name = packet.ByteBuf.ReadString();
        var address = packet.ByteBuf.ReadString();
        var population = packet.ByteBuf.ReadUnsignedInt(); // population
        var characters = packet.ByteBuf.ReadByte(); // num of characters
        var timeZone = packet.ByteBuf.ReadByte(); // timezone
        var realmId = packet.ByteBuf.ReadByte();

        // BC/wotlk include build information in the packet
        if ((realmFlags & 0x04) != 0)
        {
          var versionMajor = packet.ByteBuf.ReadByte();
          var versionMinor = packet.ByteBuf.ReadByte();
          var versionBugfix = packet.ByteBuf.ReadByte();
          var build = packet.ByteBuf.ReadUnsignedShort();
        }

        var addressTokens = address.Split(':');
        var host = addressTokens[0];
        var port = addressTokens.Length > 1 ? int.Parse(addressTokens[1]) : 8085;

        result.Add(new Realm()
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
}
