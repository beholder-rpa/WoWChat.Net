namespace WoWChat.Net.Game.Events;

using Common;

public record GameConnectingEvent : GameEvent
{
  public GameServerInfo GameServer { get; init; } = new GameServerInfo();
}