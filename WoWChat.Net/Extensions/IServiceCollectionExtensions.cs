namespace Microsoft.Extensions.DependencyInjection;

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Configuration;
using WoWChat.Net;
using WoWChat.Net.Common;
using WoWChat.Net.Options;
using WoWChat.Net.Game;
using WoWChat.Net.Realm;

public static class IServiceCollectionExtensions
{
  /// <summary>
  /// Add a WoWChat registration with the given configuration.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="config"></param>
  /// <exception cref="NotImplementedException"></exception>
  public static void AddWoWChat(this IServiceCollection services, IConfiguration config)
  {
    // Config
    var wowChatOptions = config.GetSection("WoWChat").Get<WowChatOptions>();
    services.Configure<WowChatOptions>(config.GetSection("WoWChat"));

    services.AddSingleton<IEventLoopGroup, MultithreadEventLoopGroup>();

    // Common
    services.AddSingleton<IdleStateCallback>();

    // Realm
    switch (wowChatOptions.GetExpansion())
    {
      case WoWExpansion.Vanilla:
        services.AddSingleton<RealmPacketHandler, RealmPacketHandler>();
        break;
      default:
        services.AddSingleton<RealmPacketHandler, RealmPacketHandlerTBC>();
        break;
    }

    services.AddSingleton<RealmPacketDecoder>();
    services.AddSingleton<RealmPacketEncoder>();
    services.AddSingleton<RealmChannelInitializer>();
    services.AddSingleton<RealmConnector>();

    // Game
    switch (wowChatOptions.GetExpansion())
    {
      case WoWExpansion.WotLK:
        services.AddSingleton<GamePacketEncoder, GamePacketEncoder>();
        services.AddSingleton<GamePacketDecoder, GamePacketDecoderWotLK>();
        break;
      case WoWExpansion.Cataclysm:
        throw new NotImplementedException();
      case WoWExpansion.MoP:
        throw new NotImplementedException();
      default:
        services.AddSingleton<GamePacketEncoder, GamePacketEncoder>();
        services.AddSingleton<GamePacketDecoder, GamePacketDecoder>();
        break;
    }

    switch (wowChatOptions.GetExpansion())
    {
      case WoWExpansion.Vanilla:
        services.AddSingleton<GameHeaderCrypt, GameHeaderCrypt>();
        services.AddSingleton<GamePacketHandler, GamePacketHandler>();
        break;
      case WoWExpansion.TBC:
        services.AddSingleton<GameHeaderCrypt, GameHeaderCryptTBC>();
        services.AddSingleton<GamePacketHandler, GamePacketHandlerTBC>();
        break;
      case WoWExpansion.WotLK:
        services.AddSingleton<GameHeaderCrypt, GameHeaderCryptWotLK>();
        services.AddSingleton<GamePacketHandler, GamePacketHandlerWotLK>();
        break;
      case WoWExpansion.Cataclysm:
        //socketChannel.attr(CRYPT).set(new GameHeaderCryptWotLK)
        //services.AddSingleton<GamePacketHandlerCataclysm15595>();
        break;
      case WoWExpansion.MoP:
        //socketChannel.attr(CRYPT).set(new GameHeaderCryptMoP)
        //services.AddSingleton<GamePacketHandlerMoP18414>();
        break;
    }

    services.AddSingleton<GameChannelInitializer>();
    services.AddSingleton<GameConnector>();
    

    // WoWChat
    services.AddSingleton<IWoWChat, WoWChat>();
  }
}