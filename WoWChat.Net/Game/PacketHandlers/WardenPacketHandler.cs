namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(WorldCommand.SMSG_WARDEN_DATA, WoWExpansion.Vanilla | WoWExpansion.TBC | WoWExpansion.WotLK)]
public class WardenPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<WardenPacketHandler> _logger;

  public WardenPacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<WardenPacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    if (_options.WoW.Platform == Platform.Windows)
    {
      _logger.LogError("WARDEN ON WINDOWS IS NOT SUPPORTED! BOT MAY SOON DISCONNECT! TRY TO USE PLATFORM MAC!");
      return;
    }

    throw new NotImplementedException();
  }
}