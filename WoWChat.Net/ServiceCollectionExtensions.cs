namespace WoWChat.Net;

using Common;
using DotNetty.Transport.Channels;
using Game;
using Game.PacketHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Realm;

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
    services.AddScoped<GameHeaderCrypt>();
    services.AddScoped<GameHeaderCryptTBC>();
    services.AddScoped<GameHeaderCryptWotLK>();
    services.AddScoped<GameHeaderCryptMoP>();

    services.AddScoped<GamePacketDecoder>();
    services.AddScoped<GamePacketDecoderWotLK>();
    services.AddScoped<GamePacketDecoderCataclysm>();
    services.AddScoped<GamePacketDecoderMoP>();

    services.AddScoped<GamePacketEncoder>();
    services.AddScoped<GamePacketEncoderCataclysm>();
    services.AddScoped<GamePacketEncoderMoP>();

    services.AddScoped<GameHeaderCrypt>();
    services.AddScoped<GameHeaderCryptTBC>();
    services.AddScoped<GameHeaderCryptWotLK>();

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
        WoWExpansion.Vanilla => serviceProvider.GetRequiredService<RealmPacketHandler>(),
        _ => serviceProvider.GetRequiredService<RealmPacketHandlerTBC>(),
      };
    });

    services.AddTransient<GamePacketHandlerResolver>(serviceProvider => expansion =>
    {
      return expansion switch
      {
        WoWExpansion.Vanilla => serviceProvider.GetRequiredService<GamePacketHandler>(),
        WoWExpansion.TBC => serviceProvider.GetRequiredService<GamePacketHandlerTBC>(),
        WoWExpansion.WotLK => serviceProvider.GetRequiredService<GamePacketHandlerWotLK>(),
        WoWExpansion.Cataclysm => throw new NotImplementedException(),
        WoWExpansion.MoP => throw new NotImplementedException(),
        _ => throw new NotImplementedException($"Unable to locate a game packet handler for expansion {expansion}")
      };
    });

    services.AddTransient<GamePacketDecoderResolver>(serviceProvider => expansion =>
    {
      return expansion switch
      {
        WoWExpansion.WotLK => serviceProvider.GetRequiredService<GamePacketDecoderWotLK>(),
        WoWExpansion.Cataclysm => serviceProvider.GetRequiredService<GamePacketDecoderCataclysm>(),
        WoWExpansion.MoP => serviceProvider.GetRequiredService<GamePacketDecoderMoP>(),
        _ => serviceProvider.GetRequiredService<GamePacketDecoder>()
      };
    });

    services.AddTransient<GamePacketEncoderResolver>(serviceProvider => expansion =>
    {
      return expansion switch
      {
        WoWExpansion.Cataclysm => serviceProvider.GetRequiredService<GamePacketEncoderCataclysm>(),
        WoWExpansion.MoP => serviceProvider.GetRequiredService<GamePacketEncoderMoP>(),
        _ => serviceProvider.GetRequiredService<GamePacketEncoder>()
      };
    });

    services.AddTransient<GameHeaderCryptResolver>(serviceProvider => expansion =>
    {
      return expansion switch
      {
        WoWExpansion.Vanilla => serviceProvider.GetRequiredService<GameHeaderCrypt>(),
        WoWExpansion.TBC => serviceProvider.GetRequiredService<GameHeaderCryptTBC>(),
        WoWExpansion.WotLK => serviceProvider.GetRequiredService<GameHeaderCryptWotLK>(),
        WoWExpansion.Cataclysm => serviceProvider.GetRequiredService<GameHeaderCryptWotLK>(),
        WoWExpansion.MoP => serviceProvider.GetRequiredService<GameHeaderCryptMoP>(),
        _ => throw new NotImplementedException($"Unable to locate a game header crypt handler for expansion {expansion}")
      };
    });

    //Known Packet Handlers
    services.AddScoped<ServerAuthChallengePacketHandler>();
    services.AddScoped<ServerAuthChallengePacketHandlerTBC>();
    services.AddScoped<ServerAuthChallengePacketHandlerWotLK>();

    services.AddScoped<WardenPacketHandler>();
    services.AddScoped<ServerMessagePacketHandler>();
    services.AddScoped<ServerAuthResponsePacketHandler>();
  }
}