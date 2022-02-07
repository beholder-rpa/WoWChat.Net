namespace WoWChat.Net.Game;

public partial class GamePacketHandler
{
  static readonly int[] IgnoredOpcodes = new int[]
  {
    WorldCommand.SMSG_TUTORIAL_FLAGS,
    WorldCommandWotLK.SMSG_INITIALIZE_FACTIONS,
    WorldCommandWotLK.SMSG_ACTION_BUTTONS,
    WorldCommandWotLK.SMSG_INITIAL_SPELLS,

    WorldCommand.SMSG_ADDON_INFO,
    WorldCommand.SMSG_CLIENTCACHE_VERSION,

    WorldCommandWotLK.SMSG_POWER_UPDATE,
    WorldCommandWotLK.SMSG_SET_PROFICIENCY,
    WorldCommandWotLK.SMSG_FORCE_MOVE_ROOT,
    WorldCommandWotLK.SMSG_CREATURE_QUERY_RESPONSE,
    WorldCommandWotLK.SMSG_ITEM_QUERY_SINGLE_RESPONSE,
    WorldCommandWotLK.SMSG_UPDATE_OBJECT,

    WorldCommandWotLK.SMSG_MOVE_CHARACTER_CHEAT,
    WorldCommandWotLK.SMSG_GODMODE,
    WorldCommandWotLK.SMSG_LOGIN_SET_TIME_SPEED,
    WorldCommandWotLK.SMSG_DESTROY_OBJECT,

    WorldCommandWotLK.MSG_MOVE_START_FORWARD,
    WorldCommandWotLK.MSG_MOVE_START_BACKWARD,
    WorldCommandWotLK.MSG_MOVE_STOP,
    WorldCommandWotLK.MSG_MOVE_START_STRAFE_LEFT,
    WorldCommandWotLK.MSG_MOVE_START_STRAFE_RIGHT,
    WorldCommandWotLK.MSG_MOVE_STOP_STRAFE,
    WorldCommandWotLK.MSG_MOVE_JUMP,
    WorldCommandWotLK.MSG_MOVE_START_TURN_LEFT,
    WorldCommandWotLK.MSG_MOVE_START_TURN_RIGHT,
    WorldCommandWotLK.MSG_MOVE_STOP_TURN,
    WorldCommandWotLK.MSG_MOVE_START_PITCH_UP,
    WorldCommandWotLK.MSG_MOVE_START_PITCH_DOWN,
    WorldCommandWotLK.MSG_MOVE_STOP_PITCH,
    WorldCommandWotLK.MSG_MOVE_SET_RUN_MODE,
    WorldCommandWotLK.MSG_MOVE_SET_WALK_MODE,
    WorldCommandWotLK.MSG_MOVE_STOP_STRAFE,

    WorldCommandWotLK.MSG_MOVE_FALL_LAND,
    WorldCommandWotLK.MSG_MOVE_SET_RUN_SPEED,

    WorldCommandWotLK.MSG_MOVE_SET_FACING,
    WorldCommandWotLK.SMSG_MONSTER_MOVE,
    WorldCommandWotLK.MSG_MOVE_HEARTBEAT,
    WorldCommandWotLK.SMSG_FORCE_MOVE_ROOT,

    WorldCommandWotLK.SMSG_SPELL_START,
    WorldCommandWotLK.SMSG_SPELL_GO,
    WorldCommandWotLK.SMSG_SPELL_COOLDOWN,

    WorldCommandWotLK.SMSG_ATTACK_START,
    WorldCommandWotLK.SMSG_ATTACK_STOP,
    WorldCommandWotLK.SMSG_ATTACK_SWING_NOT_IN_RANGE,
    WorldCommandWotLK.SMSG_ATTACK_SWING_BAD_FACING,

    WorldCommandWotLK.SMSG_SPELLHEALLOG,
    WorldCommandWotLK.SMSG_BIND_POINT_UPDATE,
    WorldCommandWotLK.SMSG_CLEAR_COOLDOWN,
    WorldCommandWotLK.CMSG_GM_NUKE,
    WorldCommandWotLK.SMSG_ENVIRONMENTAL_DAMAGE_LOG,

    WorldCommandWotLK.SMSG_ACCOUNT_DATA_TIMES,

    WorldCommandWotLK.SMSG_SPELLLOGMISS,
    WorldCommandWotLK.SMSG_SPELLLOGEXECUTE,

    WorldCommandWotLK.SMSG_SET_FORCED_REACTIONS,
    WorldCommandWotLK.SMSG_INIT_WORLD_STATES,
    WorldCommandWotLK.SMSG_UPDATE_WORLD_STATE,
    WorldCommandWotLK.SMSG_PLAY_SOUND,

    WorldCommandWotLK.MSG_SET_DUNGEON_DIFFICULTY,
    WorldCommandWotLK.SMSG_INSTANCE_DIFFICULTY,
    WorldCommandWotLK.SMSG_DISMOUNT,
    WorldCommandWotLK.SMSG_FEATURE_SYSTEM_STATUS,

    WorldCommandWotLK.SMSG_QUESTGIVER_STATUS_MULTIPLE,
    WorldCommandWotLK.SMSG_LEARNED_DANCE_MOVES,

    WorldCommandWotLK.SMSG_SEND_UNLEARN_SPELLS,
    WorldCommandWotLK.SMSG_ALL_ACHIEVEMENT_DATA,

    WorldCommandWotLK.SMSG_POWER_UPDATE,
    WorldCommandWotLK.SMSG_AURA_UPDATE_ALL,
    WorldCommandWotLK.SMSG_AURA_UPDATE,
    WorldCommandWotLK.SMSG_EQUIPMENT_SET_LIST,
    WorldCommandWotLK.SMSG_TALENTS_INFO,
  };
}