namespace WoWChat.Net;

using Common;
using DotNetty.Transport.Channels;
using Game;
using Game.PacketHandlers;
using global::WoWChat.Net.Game.Events;
using global::WoWChat.Net.Realm.Events;
using global::WoWChat.Net.Realm.PacketHandlers;
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

    services.AddScoped<GameChannelInitializer>();
    services.AddScoped<GameConnector>();

    // WoWChat
    services.AddScoped<IWoWChat, WoWChat>();

    // Resolvers
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

    // Add Known Packet Handlers
    var packetHandlerAttributeType = typeof(PacketHandlerAttribute);

    // Realm
    var realmPacketHandlerInterface = typeof(IPacketHandler<RealmEvent>);
    var realmPacketHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(s => s.GetTypes())
      .Where(p => realmPacketHandlerInterface.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

    foreach(var realmPacketHandlerType in realmPacketHandlerTypes)
    {
      services.AddScoped(realmPacketHandlerInterface, realmPacketHandlerType);
      services.AddScoped(realmPacketHandlerType);
    }

    // Game
    var gamePacketHandlerInterface = typeof(IPacketHandler<GameEvent>);
    var gamePacketHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(s => s.GetTypes())
      .Where(p => gamePacketHandlerInterface.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

    foreach (var gamePacketHandlerType in gamePacketHandlerTypes)
    {
      services.AddScoped(gamePacketHandlerInterface, gamePacketHandlerType);
      services.AddScoped(gamePacketHandlerType);
    }
  }
}