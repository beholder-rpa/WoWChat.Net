namespace WoWChat.Net.Game.Events;

using Common;

public record GameRetrievedCharactersEvent : GameEvent
{
  public IList<GameCharacter> Characters { get; init; } = new List<GameCharacter>();
}
