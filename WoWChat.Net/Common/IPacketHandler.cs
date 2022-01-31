namespace WoWChat.Net.Common;

using DotNetty.Transport.Channels;

public interface IPacketHandler<T>
{
  /// <summary>
  /// Gets or sets a callback that the packet handler can use to publish events.
  /// </summary>
  Action<T>? EventCallback { get; set; }

  /// <summary>
  /// Processes the indicated packet for the given context.
  /// </summary>
  /// <param name="ctx"></param>
  /// <param name="msg"></param>
  void HandlePacket(IChannelHandlerContext ctx, Packet msg);
}
