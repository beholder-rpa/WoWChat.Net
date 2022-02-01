namespace WoWChat.Net.Game.Events;

using Common;

public record GameConnectingEvent : GameEvent
{
  public GameServerInfo GameServer { get; init; } = new GameServerInfo();

  public int Port { get; init; } = 8085;

  public string SessionKey { get; init; } = string.Empty;
}