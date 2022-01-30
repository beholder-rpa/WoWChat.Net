namespace WoWChat.Net.Game.Events;

using Common;

public record GameConnectingEvent : GameEvent
{
  public GameRealm Realm { get; init; } = new GameRealm();

  public int Port { get; init; } = 8085;

  public string SessionKey { get; init; } = string.Empty;
}
