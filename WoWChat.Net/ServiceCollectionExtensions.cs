namespace WoWChat.Net;

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Configuration;
using Common;
using Options;
using Game;
using Realm;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Add a WoWChat registration with the given configuration.
  /// </summary>
  /// <param name="services"></param>
  /// <param name="config"></param>
  /// <exception cref="NotImplementedException"></exception>
  public static void AddWoWChat(this IServiceCollection services, IConfiguration namedConfigurationSection)
  {
    // Config
    services.Configure<WowChatOptions>(namedConfigurationSection);

    // Dotnetty
    services.AddScoped<IEventLoopGroup, MultithreadEventLoopGroup>();

    // Common
    services.AddScoped<IdleStateCallback>();

    // Realm
    services.AddScoped<RealmPacketHandler>();
    services.AddScoped<RealmPacketHandlerTBC>();

    services.AddScoped<RealmPacketDecoder>();
    services.AddScoped<RealmPacketEncoder>();

    services.AddScoped<RealmChannelInitializer>();
    services.AddScoped<RealmConnector>();

    // Game
    services.AddScoped<GamePacketEncoder>();

    services.AddScoped<GamePacketDecoder>();
    services.AddScoped<GamePacketDecoderWotLK>();

    services.AddScoped<GameHeaderCrypt>();
    services.AddScoped<GameHeaderCryptTBC>();
    services.AddScoped<GameHeaderCryptWotLK>();
    //socketChannel.attr(CRYPT).set(new GameHeaderCryptMoP)

    services.AddScoped<GamePacketHandler>();
    services.AddScoped<GamePacketHandlerTBC>();
    services.AddScoped<GamePacketHandlerWotLK>();
    //services.AddSingleton<GamePacketHandlerCataclysm15595>();
    //services.AddSingleton<GamePacketHandlerMoP18414>();

    services.AddScoped<GameChannelInitializer>();
    services.AddScoped<GameConnector>();

    // WoWChat
    services.AddScoped<IWoWChat, WoWChat>();

    // Resolvers
    services.AddTransient<RealmPacketHandlerResolver>(serviceProvider => expansion =>
    {
      return expansion switch
      {
        WoWExpansion.Vanilla => serviceProvider.GetService<RealmPacketHandler>(),
        _ => serviceProvider.GetService<RealmPacketHandlerTBC>(),
      };
    });
  }
}