namespace WoWChat.Net.Game.Events;

using Common;

public record GameConnectedEvent : GameEvent
{
  public GameRealm Realm { get; init; } = new GameRealm();

  public string SessionKey { get; init; } = string.Empty;
}
