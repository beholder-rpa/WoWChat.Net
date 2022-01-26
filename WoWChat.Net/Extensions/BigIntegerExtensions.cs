namespace WoWChat.Net.Extensions;

using System.Numerics;

public static class BigIntegerExtensions
{
  public static BigInteger ModPow(this BigInteger value, BigInteger pow, BigInteger mod)
  {
    return BigInteger.ModPow(value, pow, mod);
  }

  /// <summary>
  /// Removes the MSB if it is 0, then converts to a byte array.
  /// </summary>
  public static byte[] ToCleanByteArray(this BigInteger b)
  {
    byte[] array = b.ToByteArray();
    if (array[array.Length - 1] != 0)
      return array;

    byte[] temp = new byte[array.Length - 1];
    Array.Copy(array, temp, temp.Length);
    return temp;
  }
}
