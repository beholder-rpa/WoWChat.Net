namespace WoWChat.Net.Common;

using DotNetty.Transport.Channels;
using Game.Events;

public interface IPacketHandler<T>
{
  /// <summary>
  /// Gets or sets a callback that the packet handler will use to publish game events.
  /// </summary>
  Action<T>? EventCallback { get; set; }

  /// <summary>
  /// Processes the indicated packet for the given context.
  /// </summary>
  /// <param name="ctx"></param>
  /// <param name="msg"></param>
  void HandlePacket(IChannelHandlerContext ctx, Packet msg);
}
