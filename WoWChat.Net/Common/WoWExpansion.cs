namespace WoWChat.Net.Common
{
  [Flags]
  public enum WoWExpansion
  {
    Vanilla,
    TBC,
    WotLK,
    Cataclysm,
    MoP,

    All = Vanilla | TBC | WotLK | Cataclysm | MoP
  }
}
