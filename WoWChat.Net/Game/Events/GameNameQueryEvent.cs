namespace WoWChat.Net.Game.Events;

using Common;

public record GameNameQueryEvent : GameEvent
{
  public GameNameQuery NameQuery { get; init; } = new GameNameQuery();
}
