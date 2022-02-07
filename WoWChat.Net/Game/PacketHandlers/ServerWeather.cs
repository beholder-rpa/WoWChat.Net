namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Microsoft.Extensions.Logging;

[PacketHandler(WorldCommandWotLK.SMSG_WEATHER, WoWExpansion.WotLK)]
public class WeatherPacketHandler : IPacketHandler<GameEvent>
{
  protected readonly ILogger<WeatherPacketHandler> _logger;

  public WeatherPacketHandler(ILogger<WeatherPacketHandler> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var weatherType = msg.ByteBuf.ReadIntLE();
    var intensity = msg.ByteBuf.ReadFloatLE();
    var abrupt = msg.ByteBuf.ReadBoolean();

    EventCallback?.Invoke(new GameWeatherEvent()
    {
      State = (WeatherState)weatherType,
      Intensity = intensity,
      IsAbrupt = abrupt
    });

    _logger.LogDebug("SMSG_WEATHER: {type} {intensity} {abrupt}", weatherType, intensity, abrupt);
  }
}