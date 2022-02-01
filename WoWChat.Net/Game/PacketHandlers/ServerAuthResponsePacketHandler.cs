namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(WorldCommand.SMSG_AUTH_RESPONSE, WoWExpansion.All)]
public class ServerAuthResponsePacketHandler : IPacketHandler<GameEvent>
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<ServerAuthResponsePacketHandler> _logger;

  public ServerAuthResponsePacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<ServerAuthResponsePacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    EventCallback?.Invoke(new GameLoggedInEvent());
  }

  protected enum ServerMessageType : byte
  {
    SERVER_MSG_SHUTDOWN_TIME = 0x01,
    SERVER_MSG_RESTART_TIME = 0x02,
    SERVER_MSG_CUSTOM = 0x03,
    SERVER_MSG_SHUTDOWN_CANCELLED = 0x04,
    SERVER_MSG_RESTART_CANCELLED = 0x05,
  }
}