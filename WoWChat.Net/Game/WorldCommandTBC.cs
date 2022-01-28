namespace WoWChat.Net.Game;

public class WorldCommandTBC : WorldCommand
{
  public static uint SMSG_GM_MESSAGECHAT = 0x03B2;
  public static uint SMSG_MOTD = 0x033D;
  public static uint CMSG_KEEP_ALIVE = 0x0406;

  // tbc/wotlk only
  public static uint SMSG_TIME_SYNC_REQ = 0x0390;
  public static uint CMSG_TIME_SYNC_RESP = 0x0391;
}
