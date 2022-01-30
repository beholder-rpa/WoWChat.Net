namespace WoWChat.Net.Common;

using DotNetty.Buffers;

public sealed class Packet
{
  public int Id { get; private set; }

  public IByteBuffer ByteBuf { get; } = new EmptyByteBuffer(PooledByteBufferAllocator.Default);

  public Packet(byte id, IByteBuffer byteBuf)
  {
    Id = id;
    ByteBuf = byteBuf;
  }

  public Packet(int id)
  {
    Id = id;
  }

  public Packet(int id, IByteBuffer byteBuf)
  {
    Id = id;
    ByteBuf = byteBuf;
  }
}
