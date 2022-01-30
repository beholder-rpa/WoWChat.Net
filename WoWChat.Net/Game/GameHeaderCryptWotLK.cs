namespace WoWChat.Net.Game
{
  using HMACSHA1 = System.Security.Cryptography.HMACSHA1;

  public class GameHeaderCryptWotLK : GameHeaderCrypt
  {
    protected static byte[] ServerHmacSeed = new byte[] {
      0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57
    };

    protected static byte[] ClientHmacSeed = new byte[] {
      0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE
    };

    private RC4? _clientCrypt = null;
    private RC4? _serverCrypt = null;

    public GameHeaderCryptWotLK() : base()
    {
    }


    public override byte[] Decrypt(byte[] data)
    {
      if (!IsInitialized || _serverCrypt == null)
      {
        return data;
      }

      return _serverCrypt.Crypt(data);
    }

    public override byte[] Encrypt(byte[] data)
    {
      if (!IsInitialized || _clientCrypt == null)
      {
        return data;
      }

      return _clientCrypt.Crypt(data);
    }

    public override void Init(byte[] sessionKey)
    {
      using HMACSHA1 serverHmac = new HMACSHA1(ServerHmacSeed);
      var serverKey = serverHmac.ComputeHash(sessionKey);
      _serverCrypt = new RC4(serverKey);
      _serverCrypt.Crypt(new byte[1024]);

      using HMACSHA1 clientHmac = new HMACSHA1(ClientHmacSeed);
      var clientKey = clientHmac.ComputeHash(sessionKey);
      _clientCrypt = new RC4(clientKey);
      _clientCrypt.Crypt(new byte[1024]);

      IsInitialized = true;
    }
  }
}
