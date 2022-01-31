namespace WoWChat.Net.Game;

public class WorldCommand
{
  public const int CMSG_CHAR_ENUM = 0x0037;
  public const int SMSG_CHAR_ENUM = 0x003B;
  public const int CMSG_PLAYER_LOGIN = 0x003D;
  public const int CMSG_LOGOUT_REQUEST = 0x004B;
  public const int CMSG_NAME_QUERY = 0x0050;
  public const int SMSG_NAME_QUERY = 0x0051;
  public const int CMSG_GUILD_QUERY = 0x0054;
  public const int SMSG_GUILD_QUERY = 0x0055;
  public const int CMSG_WHO = 0x0062;
  public const int SMSG_WHO = 0x0063;
  public const int CMSG_GUILD_ROSTER = 0x0089;
  public const int SMSG_GUILD_ROSTER = 0x008A;
  public const int SMSG_GUILD_EVENT = 0x0092;
  public const int CMSG_MESSAGECHAT = 0x0095;
  public const int SMSG_MESSAGECHAT = 0x0096;
  public const int CMSG_JOIN_CHANNEL = 0x0097;
  public const int SMSG_CHANNEL_NOTIFY = 0x0099;

  public const int SMSG_TUTORIAL_FLAGS = 0x00FD;

  public const int SMSG_NOTIFICATION = 0x01CB;
  public const int CMSG_PING = 0x01DC;
  public const int SMSG_PONG = 0x01DD;
  public const int SMSG_AUTH_CHALLENGE = 0x01EC;
  public const int CMSG_AUTH_CHALLENGE = 0x01ED;
  public const int SMSG_AUTH_RESPONSE = 0x01EE;
  public const int SMSG_LOGIN_VERIFY_WORLD = 0x0236;
  public const int SMSG_SERVER_MESSAGE = 0x0291;

  public const int SMSG_WARDEN_DATA = 0x02E6;
  public const int CMSG_WARDEN_DATA = 0x02E7;
  public const int SMSG_ADDON_INFO = 0x02EF;

  public const int SMSG_INVALIDATE_PLAYER = 0x031C;

  public const int SMSG_CLIENTCACHE_VERSION = 0x04AB;
}
