namespace WoWChat.Net.Options;

using Common;

public class WoWOptions
{
  /// <summary>
  /// As warden is currently unimplemented, leave as Windows
  /// </summary>
  public Platform Platform { get; set; } = Platform.Windows;

  /// <summary>
  /// Optionally specify a locale if you want to join locale-specific global channels. enUS is the default locale.
  /// </summary>
  public string Locale { get; set; } = "enUS";

  /// <summary>
  /// 0 to ignore sending server's MotD. 1 to send server's MotD as a SYSTEM message.
  /// </summary>
  public bool EnableServerMOTD { get; set; } = true;

  /// <summary>
  /// Put either 1.12.1, 2.4.3, 3.3.5, 4.3.4, or 5.4.8 based on the server's expansion.
  /// </summary>
  public string Version { get; set; } = "3.3.5";

  /// <summary>
  /// If specified, denotes a custom build number.
  /// </summary>
  public ushort? Build { get; set; }

  /// <summary>
  /// Gets or sets bot's WoW game account, or set the WoWChat__WoW__AccountName environment variable.
  /// </summary>
  public string AccountName { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the bot's WoW game account password, or set the WoWChat__WoW__AccountName environment variable.
  /// </summary>
  public string AccountPassword { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the server's realmlist (logon server) host, same as in your realmlist.wtf file. Example values are logon.lightshope.org or wow.gamer-district.org
  /// </summary>
  public string RealmListHost { get; set; } = string.Empty;

  /// <summary>
  /// Port number of the logon server to use. Defaults to 3724
  /// </summary>
  public int RealmListPort { get; set; } = 3724;

  /// <summary>
  /// Gets or sets is the realm name the Bot will connect to. It is the text shown on top of character list window.
  /// </summary>
  public string RealmName { get; set; } = string.Empty;

  /// <summary>
  /// Your character's name as would be shown in the character list, or set the WOW_CHARACTER environment variable.
  /// </summary>
  public string CharacterName { get; set; } = string.Empty;

  /// <summary>
  /// Specifies the chat options
  /// </summary>
  public ChatOptions Chat { get; set; } = new ChatOptions();

  /// <summary>
  /// Specifies filters for chat messages to be ignored by the bot.
  /// </summary>
  public FilterOptions Filters { get; set; } = new FilterOptions();

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
    if (Build.HasValue && Build != 0)
    {
      return Build.Value;
    }

    return Version switch
    {
      "1.11.2" => 5464,
      "1.12.1" => 5875,
      "1.12.2" => 6005,
      "1.12.3" => 6141,
      "2.4.3" => 8606,
      "3.2.2" => 10505,
      "3.3.0" => 11159,
      "3.3.2" => 11403,
      "3.3.3" => 11723,
      "3.3.5" => 12340,
      "4.3.4" => 15595,
      "5.4.8" => 18414,
      _ => throw new ArgumentOutOfRangeException($"Build version {Version} not supported!"),
    };
  }
}