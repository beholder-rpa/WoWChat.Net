namespace WoWChat.Net.Options
{
  using Common;

  /// <summary>
  /// Represents configuration options available to WoWChat
  /// </summary>
  public class WowChatOptions
  {
    /// <summary>
    /// Gets or sets the server's realmlist host, same as in your realmlist.wtf file. Example values are logon.lightshope.org or wow.gamer-district.org
    /// </summary>
    public string RealmListHost { get; set; } = string.Empty;

    public int RealmListPort { get; set; } = 3724;

    /// <summary>
    /// Gets or sets is the realm name the Bot will connect to. It is the text shown on top of character list window.
    /// </summary>
    public string RealmName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets bot's WoW game account, or set the WoWChat__AccountName environment variable.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bot's WoW game account password, or set the WoWChat__AccountName environment variable.
    /// </summary>
    public string AccountPassword { get; set; } = string.Empty;

    public Platform Platform { get; set; } = Platform.Windows;

    public string Locale { get; set; } = "enUS";

    public string Version { get; set; } = "3.3.5";

    public int ConnectTimeoutMs { get; set; } = 10000;

    public int ReconnectDelayMs { get; set; } = 10000;

    public int ReceiveTimeoutMs { get; set; } = 10000;

    public int SendTimeoutMs { get; set; } = 10000;

    public WoWExpansion GetExpansion()
    {
      if (Version.StartsWith("1."))
      {
        return WoWExpansion.Vanilla;
      }
      else if (Version.StartsWith("2."))
      {
        return WoWExpansion.TBC;
      }
      else if (Version.StartsWith("3."))
      {
        return WoWExpansion.WotLK;
      }
      else if (Version == "4.3.4")
      {
        return WoWExpansion.Cataclysm;
      }
      else if (Version == "5.4.8")
      {
        return WoWExpansion.MoP;
      }
      else
      {
        throw new ArgumentOutOfRangeException($"Version {Version} not supported!");
      }
    }

    public ushort GetBuild()
    {
      switch (Version)
      {
        case "1.11.2":
          return 5464;
        case "1.12.1":
          return 5875;
        case "1.12.2":
          return 6005;
        case "1.12.3":
          return 6141;
        case "2.4.3":
          return 8606;
        case "3.2.2":
          return 10505;
        case "3.3.0":
          return 11159;
        case "3.3.2":
          return 11403;
        case "3.3.3":
          return 11723;
        case "3.3.5":
          return 12340;
        case "4.3.4":
          return 15595;
        case "5.4.8":
          return 18414;
        default:
          throw new ArgumentOutOfRangeException($"Build version {Version} not supported!");
      }
    }
  }
}
