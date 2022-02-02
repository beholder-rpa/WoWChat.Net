namespace WoWChat.Net.Common;

public record GameFriend
{
  public long Id { get; init; }

  public byte Status { get; init; }

  public Class Class { get; init; }

  public byte Level { get; init; }

  public int Zone { get; init; }

  public string Note { get; init; } = string.Empty;
}
