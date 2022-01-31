namespace WoWChat.Net.Common;

using DotNetty.Buffers;

public interface IPacketCommand<T>
{
  /// <summary>
  /// Gets the command id associated with the command
  /// </summary>
  int CommandId { get; set; }

  /// <summary>
  /// Gets or sets a callback that the packet command can use to publish events.
  /// </summary>
  Action<T>? EventCallback { get; set; }

  Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator);
}
