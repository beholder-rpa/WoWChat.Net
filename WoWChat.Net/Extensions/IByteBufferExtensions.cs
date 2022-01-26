namespace WoWChat.Net.Extensions;

using DotNetty.Buffers;
using System;
using System.Text;

public static class IByteBufferExtensions
{
  public static byte[] GetArrayCopy(this IByteBuffer byteBuf)
  {
    var array = new byte[byteBuf.ReadableBytes - byteBuf.ReaderIndex];
    Array.Copy(byteBuf.Copy().Array, byteBuf.ReaderIndex, array, 0, array.Length);
    return array;
  }

  public static void WriteUnsignedIntLE(this IByteBuffer byteBuf, uint data)
  {
    var dataBytes = BitConverter.GetBytes(data);
    byteBuf.WriteBytes(dataBytes);
  }

  public static void WriteStringLE(this IByteBuffer byteBuf, string str)
  {
    str = new string(str.ToCharArray().Reverse().ToArray());
    byte[] utf8StringBytes = Encoding.UTF8.GetBytes(str);
    byte[] dataBytes = new byte[utf8StringBytes.Length + 1];
    Array.Copy(utf8StringBytes, dataBytes, utf8StringBytes.Length);
    dataBytes[^1] = 0;
    byteBuf.WriteBytes(dataBytes);
  }

  public static void WriteAscii(this IByteBuffer byteBuf, string str)
  {
    byteBuf.WriteBytes(Encoding.ASCII.GetBytes(str));
  }

  public static void WriteAsciiLE(this IByteBuffer byteBuf, string str)
  {
    byteBuf.WriteBytes(Encoding.ASCII.GetBytes(str.ToCharArray().Reverse().ToArray()));
  }

  public static string ReadString(this IByteBuffer byteBuf)
  {
    var result = new List<byte>();
    while (byteBuf.ReadableBytes > 0)
    {
      var value = byteBuf.ReadByte();
      if (value == 0)
      {
        break;
      }
      result.Add(value);
    }

    return Encoding.UTF8.GetString(result.ToArray());
  }
}