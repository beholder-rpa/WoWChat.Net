namespace WoWChat.Net.Game.Events;

public record GameMessageOfTheDayEvent : GameEvent
{
  public string Message { get; set; } = string.Empty;
}