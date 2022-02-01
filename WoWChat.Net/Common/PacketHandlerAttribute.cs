namespace WoWChat.Net.Common;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PacketHandlerAttribute : Attribute
{
  public PacketHandlerAttribute(int id, WoWExpansion expansion = WoWExpansion.All)
  {
    Id = id;
    Expansion = expansion;
  }

  public int Id { get; }

  public WoWExpansion Expansion { get; }
}