namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_CHAR_ENUM, WoWExpansion.All)]
public class EnumerateCharactersCommand : IPacketCommand<GameEvent>
{
  protected readonly ILogger<KeepAliveCommand> _logger;

  public EnumerateCharactersCommand(ILogger<KeepAliveCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    _logger.LogDebug("CMSG_CHAR_ENUM");
    return Task.FromResult(new Packet(CommandId));
  }
}
