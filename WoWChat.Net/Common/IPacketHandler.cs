namespace WoWChat.Net.Common;

using DotNetty.Transport.Channels;
using Game.Events;

public interface IPacketHandler
{
  /// <summary>
  /// Gets or sets a game event that the packet handler will use to publish game events.
  /// </summary>
  Action<GameEvent>? GameEventCallback { get; set; }

  /// <summary>
  /// Processes the indicated packet for the given context.
  /// </summary>
  /// <param name="ctx"></param>
  /// <param name="msg"></param>
  void HandlePacket(IChannelHandlerContext ctx, Packet msg);
}
