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
  public static void AddWowChat(this IServiceCollection services, IConfiguration config)
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
        //socketChannel.attr(CRYPT).set(new GameHeaderCrypt)
        services.AddSingleton<GamePacketHandler, GamePacketHandler>();
        break;
      case WoWExpansion.TBC:
        //socketChannel.attr(CRYPT).set(new GameHeaderCryptTBC)
        services.AddSingleton<GamePacketHandler, GamePacketHandlerTBC>();
        break;
      case WoWExpansion.WotLK:
        //socketChannel.attr(CRYPT).set(new GameHeaderCryptWotLK)
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