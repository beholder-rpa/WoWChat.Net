namespace WoWChat.Net.Common;

using DotNetty.Buffers;

public sealed class Packet
{
  private readonly int _id;

  public int Id { get { return _id; } }

  public IByteBuffer ByteBuf { get; } = new EmptyByteBuffer(PooledByteBufferAllocator.Default);

  public Packet(byte id, IByteBuffer byteBuf)
  {
    _id = id;
    ByteBuf = byteBuf;
  }

  public Packet(int id, IByteBuffer byteBuf)
  {
    _id = id;
    ByteBuf = byteBuf;
  }
}
