namespace WoWChat.Net.Realm.Events
{
  public record RealmDisconnectedEvent : RealmEvent
  {
    public string Reason { get; init; } = string.Empty;
  }
}
