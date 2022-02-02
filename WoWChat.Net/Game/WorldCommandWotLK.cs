namespace WoWChat.Net.Game;

public class WorldCommandWotLK : WorldCommandTBC
{
  public new const int SMSG_GM_MESSAGECHAT = 0x03B3;

  public const int SMSG_SET_PROFICIENCY = 0x127;
  public const int SMSG_CREATURE_QUERY_RESPONSE = 0x061;
  public const int SMSG_ITEM_QUERY_SINGLE_RESPONSE = 0x058;
  public const int SMSG_UPDATE_OBJECT = 0x0A9;

  // World/Movement
  public const int SMSG_DESTROY_OBJECT = 0x0AA;

  public const int MSG_MOVE_START_FORWARD = 0x0B5;
  public const int MSG_MOVE_START_BACKWARD = 0x0B6;
  public const int MSG_MOVE_STOP = 0x0B7;
  public const int MSG_MOVE_START_STRAFE_LEFT = 0x0B8;
  public const int MSG_MOVE_START_STRAFE_RIGHT = 0x0B9;
  public const int MSG_MOVE_STOP_STRAFE = 0x0BA;
  public const int MSG_MOVE_JUMP = 0x0BB;
  public const int MSG_MOVE_START_TURN_LEFT = 0x0BC;
  public const int MSG_MOVE_START_TURN_RIGHT = 0x0BD;
  public const int MSG_MOVE_STOP_TURN = 0x0BE;
  public const int MSG_MOVE_START_PITCH_UP = 0x0BF;
  public const int MSG_MOVE_START_PITCH_DOWN = 0x0C0;
  public const int MSG_MOVE_STOP_PITCH = 0x0C1;
  public const int MSG_MOVE_SET_RUN_MODE = 0x0C2;
  public const int MSG_MOVE_SET_WALK_MODE = 0x0C3;
  public const int MSG_MOVE_TOGGLE_LOGGING = 0x0C4;
  public const int MSG_MOVE_TELEPORT = 0x0C5;
  public const int MSG_MOVE_TELEPORT_CHEAT = 0x0C6;
  public const int MSG_MOVE_TELEPORT_ACK = 0x0C7;
  public const int MSG_MOVE_TOGGLE_FALL_LOGGING = 0x0C8;
  public const int MSG_MOVE_FALL_LAND = 0x0C9;
  public const int MSG_MOVE_START_SWIM = 0x0CA;
  public const int MSG_MOVE_STOP_SWIM = 0x0CB;
  public const int MSG_MOVE_SET_FACING = 0x0DA;
  public const int SMSG_MONSTER_MOVE = 0x0DD;
  public const int MSG_MOVE_HEARTBEAT = 0x0EE;
  public const int SMSG_FORCE_MOVE_ROOT = 0x0E8;

  public const int SMSG_SPELL_COOLDOWN = 0x134;
  public const int SMSG_SPELLHEALLOG = 0x150;
  public const int SMSG_CLEAR_COOLDOWN = 0x1DE;
  public const int CMSG_GM_NUKE = 0x1FA;

  public const int SMSG_ACCOUNT_DATA_TIMES = 0x209;

  public const int MSG_SET_DUNGEON_DIFFICULTY = 0x329;
  public const int SMSG_FEATURE_SYSTEM_STATUS = 0x3C9;

  public new const int CMSG_KEEP_ALIVE = 0x0407;

  public const int SMSG_QUESTGIVER_STATUS_MULTIPLE = 0x0418;

  // Spells
  public const int SMSG_POWER_UPDATE = 0x480;
  public const int SMSG_AURA_UPDATE_ALL = 0x495;
  public const int SMSG_AURA_UPDATE = 0x496;
}