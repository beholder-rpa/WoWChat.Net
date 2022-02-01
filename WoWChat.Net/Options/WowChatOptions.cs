namespace WoWChat.Net.Options
{
  /// <summary>
  /// Represents configuration options available to WoWChat
  /// </summary>
  public class WowChatOptions
  {
    public WoWOptions WoW { get; set; } = new WoWOptions();

    public int ConnectTimeoutMs { get; set; } = 10000;

    public int ReconnectDelayMs { get; set; } = 10000;

    public int ReceiveTimeoutMs { get; set; } = 10000;

    public int SendTimeoutMs { get; set; } = 10000;
  }
}
