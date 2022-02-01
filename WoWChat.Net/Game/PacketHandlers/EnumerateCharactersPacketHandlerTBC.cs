namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_CHAR_ENUM, WoWExpansion.TBC)]
public class EnumerateCharactersPacketHandlerTBC : EnumerateCharactersPacketHandler
{
  public EnumerateCharactersPacketHandlerTBC(ILogger<EnumerateCharactersPacketHandlerTBC> logger)
    : base(logger)
  {
  }

  protected override IList<GameCharacter> ParseCharEnum(Packet msg)
  {
    var numberOfCharacters = msg.ByteBuf.ReadByte();

    var result = new List<GameCharacter>(numberOfCharacters);
    for (int i = 0; i < numberOfCharacters; i++)
    {
      var guid = msg.ByteBuf.ReadLongLE();
      var name = msg.ByteBuf.ReadString();
      var race = msg.ByteBuf.ReadByte(); // will determine what language to use in chat

      var charClass = msg.ByteBuf.ReadByte();
      var gender = msg.ByteBuf.ReadByte();
      var skin = msg.ByteBuf.ReadByte();
      var face = msg.ByteBuf.ReadByte();
      var hairStyle = msg.ByteBuf.ReadByte();
      var hairColor = msg.ByteBuf.ReadByte();
      var facialHair = msg.ByteBuf.ReadByte();
      var level = msg.ByteBuf.ReadByte();
      var zone = msg.ByteBuf.ReadIntLE();
      var map = msg.ByteBuf.ReadIntLE();

      var posX = msg.ByteBuf.ReadIntLE();
      var posY = msg.ByteBuf.ReadIntLE();
      var posZ = msg.ByteBuf.ReadIntLE();

      var guildGuid = msg.ByteBuf.ReadIntLE();
      var flags = msg.ByteBuf.ReadBytes(4);
      var firstLogin = msg.ByteBuf.ReadBoolean();
      var petInfo = msg.ByteBuf.ReadBytes(12);
      var equipment = msg.ByteBuf.ReadBytes(19 * 9); // equipment info TBC has 9 slot equipment info
      var bags = msg.ByteBuf.ReadBytes(9); // first bag display info TBC has 9 slot equipment info

      result.Add(new GameCharacter()
      {
        Id = guid,
        Name = name,
        Race = (Race)race,
        Class = (Class)charClass,
        Gender = (Gender)gender,
        Skin = skin,
        Face = face,
        HairStyle = hairStyle,
        HairColor = hairColor,
        FacialHair = facialHair,
        Level = level,

        Zone = zone,
        Map = map,

        PosX = posX,
        PosY = posY,
        PosZ = posZ,

        GuildId = guildGuid,
        Flags = flags.GetArrayCopy(),
        IsFirstLogin = firstLogin,

        PetInfo = petInfo.GetArrayCopy(),
        Equipment = equipment.GetArrayCopy(),
        Bags = bags.GetArrayCopy(),
      });
    }

    return result;
  }
}