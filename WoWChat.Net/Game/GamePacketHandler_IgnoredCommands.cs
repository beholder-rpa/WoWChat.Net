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


    WorldCommandWotLK.SMSG_DESTROY_OBJECT,
    WorldCommandWotLK.MSG_MOVE_STOP_STRAFE,
    WorldCommandWotLK.MSG_MOVE_JUMP,
    WorldCommandWotLK.MSG_MOVE_FALL_LAND,
    WorldCommandWotLK.MSG_MOVE_SET_FACING,
    WorldCommandWotLK.SMSG_MONSTER_MOVE,
    WorldCommandWotLK.MSG_MOVE_HEARTBEAT,
    WorldCommandWotLK.SMSG_FORCE_MOVE_ROOT,

    WorldCommandWotLK.SMSG_AURA_UPDATE_ALL,
    WorldCommandWotLK.SMSG_AURA_UPDATE,
  };
}