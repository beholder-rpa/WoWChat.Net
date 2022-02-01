namespace WoWChat.Net.Realm;

public class RealmCommand
{
  public const byte CMD_AUTH_LOGON_CHALLENGE = 0x00;
  public const byte CMD_AUTH_LOGON_PROOF = 0x01;
  public const byte CMD_REALM_LIST = 0x10;
  public const byte CMD_REALM_TRANSFER_INITIATE = 0x30;
  public const byte CMD_REALM_TRANSFER_DATA = 0x31;
  public const byte CMD_REALM_TRANSFER_ACCEPT = 0x32;
  public const byte CMD_REALM_TRANSFER_RESUME = 0x33;
  public const byte CMD_REALM_TRANSFER_CANCEL = 0x34;
}