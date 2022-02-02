namespace WoWChat.Net.Game;

public class WorldCommandWotLK : WorldCommandTBC
{
  public const int SMSG_DESTROY_OBJECT = 0x0AA;

  public new const int SMSG_GM_MESSAGECHAT = 0x03B3;
  public new const int CMSG_KEEP_ALIVE = 0x0407;

  public const int SMSG_POWER_UPDATE = 0x480;
  public const int SMSG_SET_PROFICIENCY = 0x127;
  public const int SMSG_FORCE_MOVE_ROOT = 0x0E8;
  public const int SMSG_CREATURE_QUERY_RESPONSE = 0x061;
  public const int SMSG_ITEM_QUERY_SINGLE_RESPONSE = 0x058;
  public const int SMSG_UPDATE_OBJECT = 0x0A9;
  public const int SMSG_AURA_UPDATE = 0x496;
  public const int SMSG_MONSTER_MOVE = 0x0DD;
  public const int MSG_MOVE_HEARTBEAT = 0x0EE;
}