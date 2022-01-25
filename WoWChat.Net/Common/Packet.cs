namespace WoWChat.Net.Common
{
  using System.Text;

  public sealed class Packet : IDisposable
  {
    private readonly int _commandId;
    private readonly MemoryStream _stream;
    private bool _isDisposed;

    public int CommandId { get { return _commandId; } }

    public byte[] ByteBuf { get { return _stream.ToArray(); } }

    public Packet(int commandId, int size)
    {
      _commandId = commandId;
      _stream = new MemoryStream(size);
    }

    public Packet(int commandId, byte[] byteBuf)
    {
      _commandId = commandId;
      _stream = new MemoryStream(byteBuf);
    }

    public void WriteShortLE(short data)
    {
      var dataBytes = BitConverter.GetBytes(data);
      _stream.Write(dataBytes);
    }

    public void WriteUShortLE(ushort data)
    {
      var dataBytes = BitConverter.GetBytes(data);
      _stream.Write(dataBytes);
    }

    public void WriteIntLE(int data)
    {
      var dataBytes = BitConverter.GetBytes(data);
      _stream.Write(dataBytes);
    }

    public void WriteUIntLE(uint data)
    {
      var dataBytes = BitConverter.GetBytes(data);
      _stream.Write(dataBytes);
    }

    public void WriteByte(byte data)
    {
      _stream.WriteByte(data);
    }

    public void WriteBytes(byte[] data)
    {
      _stream.Write(data);
    }

    public void WriteStringLE(string str)
    {
      str = new string(str.ToCharArray().Reverse().ToArray());
      byte[] utf8StringBytes = Encoding.UTF8.GetBytes(str);
      byte[] dataBytes = new byte[utf8StringBytes.Length + 1];
      Array.Copy(utf8StringBytes, dataBytes, utf8StringBytes.Length);
      dataBytes[^1] = 0;
      _stream.Write(dataBytes);
    }

    public void WriteAscii(string str)
    {
      _stream.Write(Encoding.ASCII.GetBytes(str));
    }

    public void WriteAsciiLE(string str)
    {
      _stream.Write(Encoding.ASCII.GetBytes(str.ToCharArray().Reverse().ToArray()));
    }

    #region IDisposable
    private void Dispose(bool disposing)
    {
      if (!_isDisposed)
      {
        if (disposing)
        {
          _stream.Dispose();
        }
        _isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
