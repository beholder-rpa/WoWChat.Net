namespace WoWChat.Net.Realm.Events
{
  public record RealmConnectingEvent : RealmEvent
  {
    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 8085;
  }
}
