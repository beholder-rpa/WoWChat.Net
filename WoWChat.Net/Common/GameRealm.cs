namespace WoWChat.Net.Common;

public record GameRealm
{
  public byte Type { get; init; }

  public byte Locked { get; init; }

  public byte Flags { get; init; }

  public string Name { get; init; } = string.Empty;

  public string Host { get; init; } = string.Empty;

  public int Port { get; init; }

  public uint Population { get; init; }

  public byte Characters { get; init; }

  public byte TimeZone { get; init; }

  public byte RealmId { get; init; }

  public Version? Version { get; init; }
}
