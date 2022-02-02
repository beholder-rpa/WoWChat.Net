namespace WoWChat.Net.Options;

using Common;

/// <summary>
/// Represents options for configuring a chat channel's notifications.
/// </summary>
public class ChannelOptions
{
  /// <summary>
  /// Gets or sets a value that indicates if the channel is enabled
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Gets or sets the channel to configure - custom channels (of type Channel) will be joined
  /// </summary>
  public ChatMessageType Type { get; set; } = ChatMessageType.Channel;

  /// <summary>
  /// Channel Name (when of kind channel)
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the message format to use.
  /// </summary>
  public string MessageFormat { get; set; } = string.Empty;
}