namespace WoWChat.Net.Game;

public class WorldCommandTBC : WorldCommand
{
  public const int SMSG_GM_MESSAGECHAT = 0x03B2;
  public const int SMSG_MOTD = 0x033D;
  public const int CMSG_KEEP_ALIVE = 0x0406;

  // tbc/wotlk only
  public const int SMSG_TIME_SYNC_REQ = 0x0390;
  public const int CMSG_TIME_SYNC_RESP = 0x0391;
}