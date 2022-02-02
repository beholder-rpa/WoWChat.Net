namespace WoWChat.Net.Common;

public record GameChatMessage
{
  public ChatMessageType MessageType { get; init; }

  public Language Language { get; init; }

  public string AddonName { get; init; } = string.Empty;

  public string ChannelName { get; init; } = string.Empty;

  public long SenderId { get; init; }

  public long TargetId { get; init; }

  public string Message { get; init; } = string.Empty;

  public string FormattedMessage { get; init; } = string.Empty;
}