namespace WoWChat.Net.Game.Events;

using Common;

public record GameChannelNotificationEvent : GameEvent
{
  public ChatNotificationKind Kind { get; set; }

  public string ChannelName { get; set; } = string.Empty;

}