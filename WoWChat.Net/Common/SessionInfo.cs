namespace WoWChat.Net.Common;
using System;

public record SessionInfo
{
  public DateTime StartTime { get; init; } = DateTime.MinValue;

  public byte[] SessionKey { get; init; } = Array.Empty<byte>();

  public int? ClientSeed { get; init; } = null;
}
