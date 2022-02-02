namespace WoWChat.Net.Options;

using Common;

public class ChannelOptions
{
  public bool Enabled = true;

  public ChatMessageType Type { get; set; } = ChatMessageType.Channel;

  public string Name { get; set; } = string.Empty;

  public string MessageFormat = string.Empty;
}