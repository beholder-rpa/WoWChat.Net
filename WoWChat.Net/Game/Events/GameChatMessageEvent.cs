namespace WoWChat.Net.Game.Events;

using Common;

public record GameChatMessageEvent : GameEvent
{
  public GameChatMessage ChatMessage { get; init; } = new GameChatMessage();
}