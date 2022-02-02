namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_LOGOUT_REQUEST, WoWExpansion.All)]
public class LogoutRequestCommand : IPacketCommand<GameEvent>
{
  protected readonly ILogger<KeepAliveCommand> _logger;

  public LogoutRequestCommand(ILogger<KeepAliveCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    _logger.LogDebug("CMSG_LOGOUT_REQUEST");
    return Task.FromResult(new Packet(CommandId));
  }
}