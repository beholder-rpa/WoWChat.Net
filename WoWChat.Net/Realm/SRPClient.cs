namespace WoWChat.Net.Realm
{
  using Extensions;
  using System.Numerics;
  using System.Security.Cryptography;
  using System.Text;

  public record SRPClient
  {
    public BigInteger ServerPrivateKey { get; }

    public BigInteger Modulus { get; }

    public BigInteger Salt { get; }

    public BigInteger Nonce { get; }

    public BigInteger A { get; }

    public BigInteger PasswordSaltedHash { get; }

    public BigInteger SessionKey { get; }

    public byte[] Proof { get; }

    public byte[] ExpectedProofResponse { get; }

    public SRPClient(
      string account,
      string password,
      byte[] b,
      byte[] g,
      byte[] n,
      byte[] salt,
      byte[] nonce,
      byte[]? a = null)
    {
      // server public key (B)
      ServerPrivateKey = b.ToBigInteger();
      // modulus (N)
      Modulus = n.ToBigInteger();
      Salt = salt.ToBigInteger();
      Nonce = nonce.ToBigInteger();
      var bigG = g.ToBigInteger();

      // hash password
      using (SHA1 alg = SHA1.Create())
      {
        string authstring = $"{account}:{password}";
        var passwordHash = alg.ComputeHash(Encoding.ASCII.GetBytes(authstring.ToUpperInvariant()));
        PasswordSaltedHash = alg.ComputeHash(salt.Combine(passwordHash)).ToBigInteger();
      }

      var rand = RandomNumberGenerator.Create();

      BigInteger keyPair;
      if (a == null)
      {
        // generate random key pair
        do
        {
          byte[] randBytes = new byte[19];
          rand.GetBytes(randBytes);
          keyPair = randBytes.ToBigInteger();

          A = bigG.ModPow(keyPair, Modulus);
        } while (A.ModPow(1, Modulus) == 0);
      }
      else
      {
        keyPair = a.ToBigInteger();
        A = bigG.ModPow(keyPair, Modulus);
      }

      // compute session key
      using (SHA1 alg = SHA1.Create())
      {
        var k = new BigInteger(3);

        var u = alg.ComputeHash(A.ToCleanByteArray().Combine(ServerPrivateKey.ToCleanByteArray())).ToBigInteger();
        var S = ((ServerPrivateKey + k * (Modulus - bigG.ModPow(PasswordSaltedHash, Modulus))) % Modulus).ModPow(keyPair + (u * PasswordSaltedHash), Modulus);

        byte[] keyHash;
        byte[] sData = S.ToCleanByteArray();
        if (sData.Length < 32)
        {
          var tmpBuffer = new byte[32];
          Buffer.BlockCopy(sData, 0, tmpBuffer, 32 - sData.Length, sData.Length);
          sData = tmpBuffer;
        }
        byte[] keyData = new byte[40];
        byte[] temp = new byte[16];

        // take every even indices byte, hash, store in even indices
        for (int i = 0; i < 16; ++i)
          temp[i] = sData[i * 2];
        keyHash = alg.ComputeHash(temp);
        for (int i = 0; i < 20; ++i)
          keyData[i * 2] = keyHash[i];

        // do the same for odd indices
        for (int i = 0; i < 16; ++i)
          temp[i] = sData[i * 2 + 1];
        keyHash = alg.ComputeHash(temp);
        for (int i = 0; i < 20; ++i)
          keyData[i * 2 + 1] = keyHash[i];

        SessionKey = keyData.ToBigInteger();

        // Generate crypto proof
        // XOR the hashes of N and g together
        byte[] gNHash = new byte[20];

        byte[] nHash = alg.ComputeHash(Modulus.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
          gNHash[i] = nHash[i];

        byte[] gHash = alg.ComputeHash(bigG.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
          gNHash[i] ^= gHash[i];

        // hash username
        byte[] userHash = alg.ComputeHash(Encoding.ASCII.GetBytes(account.ToUpperInvariant()));

        // our proof
        Proof = alg.ComputeHash
        (
            gNHash.Combine(
            userHash,
            salt,
            A.ToCleanByteArray(),
            ServerPrivateKey.ToCleanByteArray(),
            SessionKey.ToCleanByteArray()
            )
        );

        ExpectedProofResponse = alg.ComputeHash(A.ToCleanByteArray().Combine(Proof, keyData));
      }
    }
  }
}
