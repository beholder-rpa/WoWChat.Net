namespace WoWChat.Net.Game.Events;

public record GameErrorEvent : GameEvent
{
  public string Message { get; init; } = string.Empty;
}