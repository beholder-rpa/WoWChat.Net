namespace WoWChat.Net.Realm;

public class RealmAuthResult
{
  public const byte WOW_SUCCESS = 0;
  public const byte WOW_FAILURE = 0x01;
  public const byte WOW_UNKNOWN1 = 0x02;
  public const byte WOW_FAIL_BANNED = 0x03;
  public const byte WOW_FAIL_UNKNOWN_ACCOUNT = 0x04;
  public const byte WOW_FAIL_INCORRECT_PASSWORD = 0x05;
  public const byte WOW_FAIL_ALREADY_ONLINE = 0x06;
  public const byte WOW_FAIL_NO_TIME = 0x07;
  public const byte WOW_FAIL_DB_BUSY = 0x08;
  public const byte WOW_FAIL_VERSION_INVALID = 0x09;
  public const byte WOW_FAIL_VERSION_UPDATE = 0x0A;
  public const byte WOW_FAIL_INVALID_SERVER = 0x0B;
  public const byte WOW_FAIL_SUSPENDED = 0x0C;
  public const byte WOW_FAIL_FAIL_NOACCESS = 0x0D;
  public const byte WOW_SUCCESS_SURVEY = 0x0E;
  public const byte WOW_FAIL_PARENTCONTROL = 0x0F;
  public const byte WOW_FAIL_LOCKED_ENFORCED = 0x10;
  public const byte WOW_FAIL_TRIAL_ENDED = 0x11;
  public const byte WOW_FAIL_USE_BATTLENET = 0x12;
  public const byte WOW_FAIL_ANTI_INDULGENCE = 0x13;
  public const byte WOW_FAIL_EXPIRED = 0x14;
  public const byte WOW_FAIL_NO_GAME_ACCOUNT = 0x15;
  public const byte WOW_FAIL_CHARGEBACK = 0x16;
  public const byte WOW_FAIL_INTERNET_GAME_ROOM_WITHOUT_BNET = 0x17;
  public const byte WOW_FAIL_GAME_ACCOUNT_LOCKED = 0x18;
  public const byte WOW_FAIL_UNLOCKABLE_LOCK = 0x19;
  public const byte WOW_FAIL_CONVERSION_REQUIRED = 0x20;
  public const byte WOW_FAIL_DISCONNECTED = 0xFF;
}
