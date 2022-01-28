namespace WoWChat.Net.Game.Events
{
  public record GameConnectingEvent : GameEvent
  {
    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 8085;

    public string SessionKey { get; init; } = string.Empty;
  }
}
