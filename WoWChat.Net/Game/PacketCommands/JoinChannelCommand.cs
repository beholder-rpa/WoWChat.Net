namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_JOIN_CHANNEL, WoWExpansion.Vanilla)]
public class JoinChannelCommand : IPacketCommand<GameEvent>
{
  protected readonly ILogger<KeepAliveCommand> _logger;
  protected readonly GameChannelLookup _channels;

  public JoinChannelCommand(GameChannelLookup channels, ILogger<KeepAliveCommand> logger)
  {
    _channels = channels ?? throw new ArgumentNullException(nameof(channels));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public int ChannelId { get; set; }

  public string ChannelName { get; set; } = string.Empty;

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    if (string.IsNullOrWhiteSpace(ChannelName))
    {
      throw new InvalidOperationException("ChannelName must be specified.");
    }

    if (_channels.TryGetChannel(ChannelId, out _))
    {
      throw new InvalidOperationException($"A channel id {ChannelId} has already been used. ({ChannelName})");
    }

    lock (_channels)
    {
      if (_channels.TryGetChannel(ChannelId, out _))
      {
        throw new InvalidOperationException($"A channel id {ChannelId} has already been used. ({ChannelName})");
      }

      _logger.LogDebug("Joining Channel {channelName}", ChannelName);
      var byteBuf = allocator.Buffer(50, 200);
      WriteJoinChannel(byteBuf, ChannelId, ChannelName);

      _channels.AddOrUpdate(ChannelId, ChannelName);
      return Task.FromResult(new Packet(CommandId));
    }
  }

  protected virtual void WriteJoinChannel(IByteBuffer byteBuf, int id, string channelName)
  {
    byteBuf.WriteStringLE(channelName);
    byteBuf.WriteByte(0);
    byteBuf.WriteByte(0);
  }
}