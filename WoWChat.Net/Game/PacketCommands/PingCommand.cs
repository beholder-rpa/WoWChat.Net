namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_PING, WoWExpansion.All)]
public class PingCommand : IPacketCommand<GameEvent>
{
  protected readonly Random _random = new();
  protected int _pingId = 0;

  protected readonly ILogger<PingCommand> _logger;

  public PingCommand(ILogger<PingCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    var latency = _random.Next(50) + 90;

    var byteBuf = allocator.Buffer(8, 8);
    byteBuf.WriteIntLE(_pingId);
    byteBuf.WriteIntLE(latency);
    _logger.LogDebug("PING: {pingId}", _pingId);
    _pingId += 1;
    return Task.FromResult(new Packet(CommandId, byteBuf));
  }
}
