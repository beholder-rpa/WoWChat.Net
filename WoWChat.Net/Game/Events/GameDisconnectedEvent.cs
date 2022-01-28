namespace WoWChat.Net.Game.Events
{
  public record GameDisconnectedEvent : GameEvent
  {
    public bool AutoReconnect { get; init; } = true;

    public bool IsExpected { get; init; } = false;

    public string Reason { get; init; } = string.Empty;
  }
}
