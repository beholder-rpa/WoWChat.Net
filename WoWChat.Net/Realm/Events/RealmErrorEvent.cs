namespace WoWChat.Net.Realm.Events
{
  public record RealmErrorEvent : RealmEvent
  {
    public string Message { get; init; } = string.Empty;
  }
}