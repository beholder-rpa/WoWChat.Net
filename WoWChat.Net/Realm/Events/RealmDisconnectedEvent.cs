namespace WoWChat.Net.Realm.Events
{
  public record RealmDisconnectedEvent : RealmEvent
  {
    public bool AutoReconnect { get; init; } = true;

    public bool IsExpected { get; init; } = false;

    public string Reason { get; init; } = string.Empty;
  }
}
