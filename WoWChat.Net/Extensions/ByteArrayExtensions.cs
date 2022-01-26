namespace WoWChat.Net.Extensions;

using System.Numerics;

public static class ByteArrayExtensions
{
  /// <summary>
  /// places a non-negative value (0) at the MSB, then converts to a BigInteger.
  /// This ensures a non-negative value without changing the binary representation.
  /// </summary>
  public static BigInteger ToBigInteger(this byte[] array)
  {
    byte[] temp;
    if ((array[^1] & 0x80) == 0x80)
    {
      temp = new byte[array.Length + 1];
      temp[array.Length] = 0;
    }
    else
      temp = new byte[array.Length];

    Array.Copy(array, temp, array.Length);
    return new BigInteger(temp);
  }

  /// <summary>
  /// Combines all specified byte arrays into a single byte array
  /// </summary>
  /// <param name="buffers"></param>
  /// <returns></returns>
  public static byte[] Combine(this byte[] array, params byte[][] arrays)
  {
    var bytes = new byte[array.Length + arrays.Sum(a => a.Length)];
    int offset = 0;

    Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
    offset += array.Length;

    foreach (byte[] subArray in arrays)
    {
      Buffer.BlockCopy(subArray, 0, bytes, offset, subArray.Length);
      offset += subArray.Length;
    }

    return bytes;
  }
}
