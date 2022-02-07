namespace WoWChat.Net.Game;

public class WorldCommandWotLK : WorldCommandTBC
{
  public new const int SMSG_GM_MESSAGECHAT = 0x03B3;

  public const int SMSG_INITIALIZE_FACTIONS = 0x122;
  public const int SMSG_SET_PROFICIENCY = 0x127;
  public const int SMSG_ACTION_BUTTONS = 0x129;
  public const int SMSG_INITIAL_SPELLS = 0x12A;
  public const int SMSG_CREATURE_QUERY_RESPONSE = 0x061;
  public const int SMSG_CONTACT_LIST = 0x067;
  public const int SMSG_ITEM_QUERY_SINGLE_RESPONSE = 0x058;
  public const int SMSG_UPDATE_OBJECT = 0x0A9;

  // World/Movement
  public const int SMSG_MOVE_CHARACTER_CHEAT = 0x00E;
  public const int SMSG_GODMODE = 0x023;
  public const int SMSG_LOGIN_SET_TIME_SPEED = 0x042;
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
  public const int MSG_MOVE_SET_RUN_SPEED = 0x0CD;
  public const int MSG_MOVE_SET_FACING = 0x0DA;
  public const int SMSG_MONSTER_MOVE = 0x0DD;
  public const int MSG_MOVE_HEARTBEAT = 0x0EE;
  public const int SMSG_FORCE_MOVE_ROOT = 0x0E8;

  public const int SMSG_SPELL_START = 0x131;
  public const int SMSG_SPELL_GO = 0x132;
  public const int SMSG_SPELL_COOLDOWN = 0x134;

  public const int CMSG_ATTACK_SWING = 0x141;
  public const int CMSG_ATTACK_STOP = 0x142;
  public const int SMSG_ATTACK_START = 0x143;
  public const int SMSG_ATTACK_STOP = 0x144;
  public const int SMSG_ATTACK_SWING_NOT_IN_RANGE = 0x145;
  public const int SMSG_ATTACK_SWING_BAD_FACING = 0x146;

  public const int SMSG_SPELLHEALLOG = 0x150;
  public const int SMSG_BIND_POINT_UPDATE = 0x155;
  public const int SMSG_CLEAR_COOLDOWN = 0x1DE;
  public const int CMSG_GM_NUKE = 0x1FA;
  public const int SMSG_ENVIRONMENTAL_DAMAGE_LOG = 0x1FC;

  public const int SMSG_ACCOUNT_DATA_TIMES = 0x209;
  
  public const int SMSG_SPELLLOGMISS = 0x24B;
  public const int SMSG_SPELLLOGEXECUTE = 0x24C;

  public const int SMSG_ZONE_UNDER_ATTACK = 0x254;
  public const int SMSG_SET_FORCED_REACTIONS = 0x2A5;
  public const int SMSG_INIT_WORLD_STATES = 0x2C2;
  public const int SMSG_UPDATE_WORLD_STATE = 0x2C3;
  public const int SMSG_PLAY_SOUND = 0x2D2;
  public const int SMSG_WEATHER = 0x2F4;

  public const int MSG_SET_DUNGEON_DIFFICULTY = 0x329;
  public const int SMSG_INSTANCE_DIFFICULTY = 0x033B;
  public const int SMSG_DISMOUNT = 0x3AC;
  public const int SMSG_FEATURE_SYSTEM_STATUS = 0x3C9;

  public new const int CMSG_KEEP_ALIVE = 0x0407;

  public const int SMSG_QUESTGIVER_STATUS_MULTIPLE = 0x0418;
  public const int SMSG_LEARNED_DANCE_MOVES = 0x455;

  
  public const int SMSG_SEND_UNLEARN_SPELLS = 0x41E;

  public const int SMSG_ALL_ACHIEVEMENT_DATA = 0x47D;

  // Spells
  public const int SMSG_POWER_UPDATE = 0x480;
  public const int SMSG_AURA_UPDATE_ALL = 0x495;
  public const int SMSG_AURA_UPDATE = 0x496;
  public const int SMSG_EQUIPMENT_SET_LIST = 0x4BC; // equipment manager list?
  public const int SMSG_TALENTS_INFO = 0x4C0;
}