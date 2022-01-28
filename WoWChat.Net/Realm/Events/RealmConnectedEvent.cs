namespace WoWChat.Net.Realm.Events
{
  public record RealmConnectedEvent : RealmEvent
  {
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 8085;

    public string SessionKey { get; init; } = string.Empty;
  }
}
