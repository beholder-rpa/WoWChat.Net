namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_NAME_QUERY, WoWExpansion.All)]
public class GameNameQueryCommand : IPacketCommand<GameEvent>
{
  protected readonly ILogger<GameNameQueryCommand> _logger;

  public GameNameQueryCommand(ILogger<GameNameQueryCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public long Guid { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    var byteBuf = allocator.Buffer(8, 8);
    byteBuf.WriteLongLE(Guid);
    _logger.LogDebug("CMSG_NAME_QUERY: {pingId}", Guid);
    return Task.FromResult(new Packet(CommandId, byteBuf));
  }
}