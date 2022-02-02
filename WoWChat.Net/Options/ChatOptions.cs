namespace WoWChat.Net.Options;

public class ChatOptions
{
  /// <summary>
  /// Indicates if chat is enabled
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Indicates the channels to auto-join
  /// </summary>
  public ChannelOptions[] Channels { get; set; } = Array.Empty<ChannelOptions>();
}