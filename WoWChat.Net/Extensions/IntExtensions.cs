namespace WoWChat.Net.Extensions
{
  public static class IntExtensions
  {

    public static byte[] ToBytesShort(this int s)
    {
      return new byte[]
      {
        (byte)(s >> 8),
        (byte)s
      };
    }

    public static byte[] ToBytesShortLE(this int s)
    {
      return new byte[]
      {
        (byte)s,
        (byte)(s >> 8),
      };
    }

    /// <summary>
    /// Returns a Big-Endian byte array representation of the integer.
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this int a)
    {
      return new byte[]
      {
        (byte)(a >> 24),
        (byte)(a >> 16),
        (byte)(a >> 8),
        (byte)a,
      };
    }

    /// <summary>
    /// Returns a Little Endian byte array representation of the integer.
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static byte[] ToBytesLE(this int a)
    {
      return new byte[]
      {
        (byte)a,
        (byte)(a >> 8),
        (byte)(a >> 16),
        (byte)(a >> 24),
      };
    }
  }
}