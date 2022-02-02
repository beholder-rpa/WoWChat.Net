namespace WoWChat.Net.Game.Events;

using Common;

public record GameServerMessageEvent : GameEvent
{
  public ServerMessageKind Kind { get; set; }

  public string Message { get; set; } = string.Empty;
}