namespace WoWChat.Net.Tests
{
  using Extensions;
  using System;
  using System.Linq;
  using Xunit;

  public class IntExtensionTests
  {
    [Fact]
    public void ToByteIsBigEndian()
    {
      var foo = BitConverter.ToString(12345.ToBytes());
      var bar = BitConverter.ToString(BitConverter.GetBytes(12345).Reverse().ToArray());

      Assert.Equal(bar, foo);
    }

    [Fact]
    public void ToByteLEIsLittleEndian()
    {
      var foo = BitConverter.ToString(12345.ToBytesLE());
      var bar = BitConverter.ToString(BitConverter.GetBytes(12345));

      Assert.Equal(bar, foo);
    }
  }
}