namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommand.SMSG_NAME_QUERY, WoWExpansion.Vanilla | WoWExpansion.TBC | WoWExpansion.WotLK)]
//[PacketHandler(WorldCommandCataclysm.SMSG_NAME_QUERY, WoWExpansion.Cataclysm)]
//[PacketHandler(WorldCommandMoP.SMSG_NAME_QUERY, WoWExpansion.MoP)]
public class ServerNameQueryPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<ServerNameQueryPacketHandler> _logger;

  public ServerNameQueryPacketHandler(ILogger<ServerNameQueryPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var query = ParseNameQuery(msg);
    EventCallback?.Invoke(new GameNameQueryEvent()
    {
      NameQuery = query
    });
    _logger.LogDebug("SMSG_NAME_QUERY");
  }

  protected virtual GameNameQuery ParseNameQuery(Packet msg)
  {
    var id = msg.ByteBuf.ReadPackedGuid();
    var end = msg.ByteBuf.ReadBoolean();
    if (end)
    {
      return new GameNameQuery() { Id = (long)id };
    }

    var name = msg.ByteBuf.ReadString();
    var realmName = msg.ByteBuf.ReadString();
    var race = msg.ByteBuf.ReadByte();
    var gender = msg.ByteBuf.ReadByte();
    var charClass = msg.ByteBuf.ReadByte();

    return new GameNameQuery()
    {
      Id = (long)id,
      Name = name,
      RealmName = realmName,
      Race = (Race)race,
      Gender = (Gender)gender,
      Class = (Class)charClass,
    };
  }
}
