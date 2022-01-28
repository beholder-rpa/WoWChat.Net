namespace WoWChat.Net.Realm.Events
{
  public record RealmListEvent : RealmEvent
  {
    public IList<Realm> RealmList { get; init; } = new List<Realm>();
  }
}
