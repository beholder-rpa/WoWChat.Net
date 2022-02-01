namespace WoWChat.Net.Realm.Events
{
  public record RealmAuthenticatedEvent : RealmEvent
  {
    public byte[] SessionKey { get; init; } = Array.Empty<byte>();

    public int AccountFlag { get; init; }
  }
}