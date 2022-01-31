namespace WoWChat.Net.Game.Events;

public record GameInvalidatePlayerEvent : GameEvent
{
  public long PlayerId { get; init; }
}
