namespace WoWChat.Net.Realm.Events
{
  using System.Numerics;

  public record RealmAuthenticatedEvent : RealmEvent
  {
    public BigInteger SessionKey { get; init; }

    public int AccountFlag { get; init; }
  }
}
