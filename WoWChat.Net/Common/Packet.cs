namespace WoWChat.Net.Common;

using DotNetty.Buffers;
using Realm;

public sealed class Packet
{
  private readonly byte _id;

  public byte Id { get { return _id; } }

  public IByteBuffer ByteBuf { get; } = new EmptyByteBuffer(PooledByteBufferAllocator.Default);

  public Packet(RealmAuthCommand cmd, IByteBuffer byteBuf)
    : this((byte)cmd, byteBuf)
  {
  }

  public Packet(byte id, IByteBuffer byteBuf)
  {
    _id = id;
    ByteBuf = byteBuf;
  }
}
