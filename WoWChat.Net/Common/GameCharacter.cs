namespace WoWChat.Net.Common;

public record GameCharacter
{
  public long Id { get; init; }

  public string Name { get; init; } = String.Empty;

  public Race Race { get; init; }

  public Class Class { get; init; }

  public Gender Gender { get; init; }

  public byte Skin { get; init; }

  public byte Face { get; init; }

  public byte HairStyle { get; init; }

  public byte HairColor { get; init; }

  public byte FacialHair { get; init; }

  public byte Level { get; init; }

  public int Zone { get; init; }

  public int Map { get; init; }

  public int PosX { get; init; }

  public int PosY { get; init; }

  public int PosZ { get; init; }

  public int GuildId { get; init; }

  public byte[] Flags { get; init; } = Array.Empty<byte>();

  public byte[] CustomizeFlags { get; init; } = Array.Empty<byte>();

  public bool IsFirstLogin { get; init; }

  public byte[] PetInfo { get; init; } = Array.Empty<byte>();

  public byte[] Equipment { get; init; } = Array.Empty<byte>();

  public byte[] Bags { get; init; } = Array.Empty<byte>();
}
