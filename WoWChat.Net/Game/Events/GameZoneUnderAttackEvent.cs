namespace WoWChat.Net.Game.Events;

public record GameZoneUnderAttackEvent : GameEvent
{
  public int ZoneId { get; init; }
}