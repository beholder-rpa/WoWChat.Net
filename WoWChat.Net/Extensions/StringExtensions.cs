namespace WoWChat.Net
{
  using System;
  using System.Text;

  public static class StringExtensions
  {
    public static string ReadCString(this BinaryReader reader)
    {
      StringBuilder builder = new StringBuilder();

      while (true)
      {
        byte letter = reader.ReadByte();
        if (letter == 0)
          break;

        builder.Append((char)letter);
      }

      return builder.ToString();
    }

    public static byte[] ToCString(this string str)
    {
      byte[] utf8StringBytes = Encoding.UTF8.GetBytes(str);
      byte[] data = new byte[utf8StringBytes.Length + 1];
      Array.Copy(utf8StringBytes, data, utf8StringBytes.Length);
      data[^1] = 0;
      return data;
    }

    public static byte[] ToCStringLE(this string str)
    {
      return ToCString(new string(str.ToCharArray().Reverse().ToArray()));
    }
  }
}