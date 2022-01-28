namespace WoWChat.Net.Game;

public class WorldCommand
{
  public static uint CMSG_CHAR_ENUM = 0x37;
  public static uint SMSG_CHAR_ENUM = 0x3B;
  public static uint CMSG_PLAYER_LOGIN = 0x3D;
  public static uint CMSG_LOGOUT_REQUEST = 0x4B;
  public static uint CMSG_NAME_QUERY = 0x50;
  public static uint SMSG_NAME_QUERY = 0x51;
  public static uint CMSG_GUILD_QUERY = 0x54;
  public static uint SMSG_GUILD_QUERY = 0x55;
  public static uint CMSG_WHO = 0x62;
  public static uint SMSG_WHO = 0x63;
  public static uint CMSG_GUILD_ROSTER = 0x89;
  public static uint SMSG_GUILD_ROSTER = 0x8A;
  public static uint SMSG_GUILD_EVENT = 0x92;
  public static uint CMSG_MESSAGECHAT = 0x95;
  public static uint SMSG_MESSAGECHAT = 0x96;
  public static uint CMSG_JOIN_CHANNEL = 0x97;
  public static uint SMSG_CHANNEL_NOTIFY = 0x99;

  public static uint SMSG_NOTIFICATION = 0x01CB;
  public static uint CMSG_PING = 0x01DC;
  public static uint SMSG_AUTH_CHALLENGE = 0x01EC;
  public static uint CMSG_AUTH_CHALLENGE = 0x01ED;
  public static uint SMSG_AUTH_RESPONSE = 0x01EE;
  public static uint SMSG_LOGIN_VERIFY_WORLD = 0x0236;
  public static uint SMSG_SERVER_MESSAGE = 0x0291;

  public static uint SMSG_WARDEN_DATA = 0x02E6;
  public static uint CMSG_WARDEN_DATA = 0x02E7;

  public static uint SMSG_INVALIDATE_PLAYER = 0x031C;

  // tbc/wotlk only
  public static uint SMSG_TIME_SYNC_REQ = 0x0390;
  public static uint CMSG_TIME_SYNC_RESP = 0x0391;
}
