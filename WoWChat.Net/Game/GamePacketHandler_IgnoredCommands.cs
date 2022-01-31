namespace WoWChat.Net.Game;

public partial class GamePacketHandler
{
  static readonly int[] IgnoredOpcodes = new int[]
  {
    WorldCommand.SMSG_TUTORIAL_FLAGS,
    WorldCommand.SMSG_ADDON_INFO,
    WorldCommand.SMSG_CLIENTCACHE_VERSION,

    WorldCommandWotLK.SMSG_POWER_UPDATE,
    WorldCommandWotLK.SMSG_SET_PROFICIENCY,
    WorldCommandWotLK.SMSG_FORCE_MOVE_ROOT,
    WorldCommandWotLK.SMSG_CREATURE_QUERY_RESPONSE,
    WorldCommandWotLK.SMSG_ITEM_QUERY_SINGLE_RESPONSE,
    WorldCommandWotLK.SMSG_UPDATE_OBJECT,
  };
}
