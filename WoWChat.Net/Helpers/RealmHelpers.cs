namespace WoWChat.Net.Helpers;

using Realm;

public static class RealmHelpers
{
  public static bool IsAuthResultSuccess(byte value)
  {
    if ((byte)RealmAuthResult.WOW_SUCCESS == value || (byte)RealmAuthResult.WOW_SUCCESS_SURVEY == value)
    {
      return true;
    }

    return false;
  }

  public static string GetMessage(byte authResult)
  {
    return authResult switch
    {
      RealmAuthResult.WOW_SUCCESS or RealmAuthResult.WOW_SUCCESS_SURVEY => "Success!",
      RealmAuthResult.WOW_FAIL_BANNED => "Your account has been banned!",
      RealmAuthResult.WOW_FAIL_INCORRECT_PASSWORD => "Incorrect username or password!",
      RealmAuthResult.WOW_FAIL_UNKNOWN_ACCOUNT => "Unknown Account Name.",
      RealmAuthResult.WOW_FAIL_ALREADY_ONLINE => "Your account is already online. Wait a moment and try again!",
      RealmAuthResult.WOW_FAIL_VERSION_INVALID or RealmAuthResult.WOW_FAIL_VERSION_UPDATE => "Invalid game version for this server!",
      RealmAuthResult.WOW_FAIL_SUSPENDED => "Your account has been suspended!",
      RealmAuthResult.WOW_FAIL_FAIL_NOACCESS => "Login failed! You do not have access to this server!",
      _ => $"Failed to login to realm server! Error code: {authResult:X2}",
    };
  }
}
