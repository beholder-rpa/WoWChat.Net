namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommandTBC.CMSG_KEEP_ALIVE, WoWExpansion.TBC | WoWExpansion.WotLK)]
public class KeepAliveCommand : IPacketCommand<GameEvent>
{
  protected readonly ILogger<KeepAliveCommand> _logger;

  public KeepAliveCommand(ILogger<KeepAliveCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    _logger.LogDebug("KEEP ALIVE");
    return Task.FromResult(new Packet(CommandId));
  }
}
