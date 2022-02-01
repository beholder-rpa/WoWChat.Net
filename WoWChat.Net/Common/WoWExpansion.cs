namespace WoWChat.Net.Common
{
  [Flags]
  public enum WoWExpansion
  {
    None = 0,
    Vanilla = 1,
    TBC = 2,
    WotLK = 4,
    Cataclysm = 8,
    MoP = 16,

    All = Vanilla | TBC | WotLK | Cataclysm | MoP
  }
}