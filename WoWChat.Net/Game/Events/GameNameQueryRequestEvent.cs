namespace WoWChat.Net.Game.Events;

using Common;

public record GameNameQueryRequestEvent : GameEvent
{
  public long Guid { get; init; }
}