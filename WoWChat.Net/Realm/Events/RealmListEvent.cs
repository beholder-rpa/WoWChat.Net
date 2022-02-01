namespace WoWChat.Net.Realm.Events
{
  using Common;

  public record RealmListEvent : RealmEvent
  {
    public IList<GameServerInfo> RealmList { get; init; } = new List<GameServerInfo>();
  }
}