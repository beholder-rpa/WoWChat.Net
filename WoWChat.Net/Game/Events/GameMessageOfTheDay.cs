namespace WoWChat.Net.Game.Events;

public record GameMessageOfTheDay : GameEvent
{
  public string Message { get; set; } = string.Empty;
}