namespace WoWChat.Net.Realm.Events
{
  using Common;

  public record RealmListEvent : RealmEvent
  {
    public IList<GameRealm> RealmList { get; init; } = new List<GameRealm>();
  }
}
