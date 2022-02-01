namespace WoWChat.Net.Game
{
  using HMACSHA1 = System.Security.Cryptography.HMACSHA1;

  public class GameHeaderCryptTBC : GameHeaderCrypt
  {
    public override void Init(byte[] sessionKey)
    {
      base.Init(sessionKey);

      var hmacSeed = new byte[] { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x04, 0xE2, 0xAA };

      using HMACSHA1 outputHMAC = new HMACSHA1(hmacSeed);
      _key = outputHMAC.ComputeHash(sessionKey);
    }
  }
}