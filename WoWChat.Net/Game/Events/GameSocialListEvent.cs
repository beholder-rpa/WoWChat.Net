namespace WoWChat.Net.Game.Events;

using Common;

public record GameSocialListEvent : GameEvent
{
  public byte Kind { get; init; }

  public IList<GameFriend> Friends { get; init; } = new List<GameFriend>();
}