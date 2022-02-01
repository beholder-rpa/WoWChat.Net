namespace WoWChat.Net.Game.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;
using Microsoft.Extensions.Logging;

[PacketCommand(WorldCommand.CMSG_PLAYER_LOGIN, WoWExpansion.Vanilla | WoWExpansion.TBC | WoWExpansion.WotLK)]
public class PlayerLoginCommand : IPacketCommand<GameEvent>
{
  protected readonly Random _random = new();
  protected int _pingId = 0;

  protected readonly ILogger<PingCommand> _logger;

  public PlayerLoginCommand(ILogger<PingCommand> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public int CommandId { get; set; }

  public GameCharacter? Character { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    var byteBuf = allocator.Buffer(16, 16); // increase to 16 for MoP
    WritePlayerLogin(byteBuf);
    return Task.FromResult(new Packet(CommandId, byteBuf));
  }

  protected virtual void WritePlayerLogin(IByteBuffer byteBuf)
  {
    if (Character == null)
    {
      throw new InvalidOperationException("Character must be specified.");
    }

    byteBuf.WriteLongLE(Character.Id);
  }
}