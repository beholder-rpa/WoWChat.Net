namespace WoWChat.Net.Game;

public class WorldCommand
{
  public const int CMSG_CHAR_ENUM = 0x37;
  public const int SMSG_CHAR_ENUM = 0x3B;
  public const int CMSG_PLAYER_LOGIN = 0x3D;
  public const int CMSG_LOGOUT_REQUEST = 0x4B;
  public const int CMSG_NAME_QUERY = 0x50;
  public const int SMSG_NAME_QUERY = 0x51;
  public const int CMSG_GUILD_QUERY = 0x54;
  public const int SMSG_GUILD_QUERY = 0x55;
  public const int CMSG_WHO = 0x62;
  public const int SMSG_WHO = 0x63;
  public const int CMSG_GUILD_ROSTER = 0x89;
  public const int SMSG_GUILD_ROSTER = 0x8A;
  public const int SMSG_GUILD_EVENT = 0x92;
  public const int CMSG_MESSAGECHAT = 0x95;
  public const int SMSG_MESSAGECHAT = 0x96;
  public const int CMSG_JOIN_CHANNEL = 0x97;
  public const int SMSG_CHANNEL_NOTIFY = 0x99;

  public const int SMSG_NOTIFICATION = 0x01CB;
  public const int CMSG_PING = 0x01DC;
  public const int SMSG_AUTH_CHALLENGE = 0x01EC;
  public const int CMSG_AUTH_CHALLENGE = 0x01ED;
  public const int SMSG_AUTH_RESPONSE = 0x01EE;
  public const int SMSG_LOGIN_VERIFY_WORLD = 0x0236;
  public const int SMSG_SERVER_MESSAGE = 0x0291;

  public const int SMSG_WARDEN_DATA = 0x02E6;
  public const int CMSG_WARDEN_DATA = 0x02E7;

  public const int SMSG_INVALIDATE_PLAYER = 0x031C;
}
