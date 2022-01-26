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
      (byte)RealmAuthResult.WOW_SUCCESS or (byte)RealmAuthResult.WOW_SUCCESS_SURVEY => "Success!",
      (byte)RealmAuthResult.WOW_FAIL_BANNED => "Your account has been banned!",
      (byte)RealmAuthResult.WOW_FAIL_INCORRECT_PASSWORD => "Incorrect username or password!",
      (byte)RealmAuthResult.WOW_FAIL_UNKNOWN_ACCOUNT => "Unknown Account Name.",
      (byte)RealmAuthResult.WOW_FAIL_ALREADY_ONLINE => "Your account is already online. Wait a moment and try again!",
      (byte)RealmAuthResult.WOW_FAIL_VERSION_INVALID or (byte)RealmAuthResult.WOW_FAIL_VERSION_UPDATE => "Invalid game version for this server!",
      (byte)RealmAuthResult.WOW_FAIL_SUSPENDED => "Your account has been suspended!",
      (byte)RealmAuthResult.WOW_FAIL_FAIL_NOACCESS => "Login failed! You do not have access to this server!",
      _ => $"Failed to login to realm server! Error code: {authResult:X2}",
    };
  }
}
