namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommandWotLK.SMSG_CONTACT_LIST, WoWExpansion.WotLK)]
public class SocialListPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<SocialListPacketHandler> _logger;

  public SocialListPacketHandler(ILogger<SocialListPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var listKind = msg.ByteBuf.ReadByte();
    var count = msg.ByteBuf.ReadIntLE();

    var friends = new List<GameFriend>();
    for(int i = 0; i < count; i++)
    {
      var friendGuid = msg.ByteBuf.ReadLongLE();
      var kind = msg.ByteBuf.ReadIntLE();
      var note = msg.ByteBuf.ReadString();

      int friendZone = default;
      byte friendLevel = default;
      byte friendClass = default;
      byte onlineStatus = default;
      if (kind == (int)SocialListKind.FriendList)
      {
        onlineStatus = msg.ByteBuf.ReadByte();
        if (onlineStatus > 0)
        {
          friendZone = msg.ByteBuf.ReadIntLE();
          friendLevel = msg.ByteBuf.ReadByte();
          friendClass = msg.ByteBuf.ReadByte();
        }
      }

      friends.Add(new GameFriend()
      {
        Id = friendGuid,
        Status = onlineStatus,
        Zone = friendZone,
        Level = friendLevel,
        Class = (Class)friendClass,
        Note = note,
      });
    }

    EventCallback?.Invoke(new GameSocialListEvent()
    {
      Kind = listKind,
      Friends = friends,
    });
    _logger.LogDebug("SMSG_CONTACT_LIST: {listKind} {count}", listKind, count);
  }

  private enum SocialListKind : byte
  {
    FriendList = 0x01,
    IgnoreList = 0x02,
    MuteList = 0x04,
  }
}