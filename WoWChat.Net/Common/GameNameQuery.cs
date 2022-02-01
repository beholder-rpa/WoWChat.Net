namespace WoWChat.Net.Common
{
  public record GameNameQuery
  {
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string RealmName { get; init; } = string.Empty;

    public Race Race { get; init; }

    public Gender Gender { get; init; }

    public Class Class { get; init; }
  }
}