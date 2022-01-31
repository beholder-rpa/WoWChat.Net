namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_CHAR_ENUM, WoWExpansion.Vanilla)]
public class EnumerateCharactersPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<EnumerateCharactersPacketHandler> _logger;

  public EnumerateCharactersPacketHandler(ILogger<EnumerateCharactersPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var characters = ParseCharEnum(msg);
    EventCallback?.Invoke(new GameRetrievedCharactersEvent()
    {
      Characters = characters
    });
    _logger.LogDebug("SMSG_CHAR_ENUM - {numCharacters} retrieved", characters.Count);
  }

  protected virtual IList<GameCharacter> ParseCharEnum(Packet msg) {

    var numberOfCharacters = msg.ByteBuf.ReadByte();

    var result = new List<GameCharacter>(numberOfCharacters);
    for(int i = 0; i < numberOfCharacters; i++)
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
      var equipment = msg.ByteBuf.ReadBytes(19 * 5);
      var bags = msg.ByteBuf.ReadBytes(5);

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
