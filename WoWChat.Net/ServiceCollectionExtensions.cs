namespace WoWChat.Net;

using Common;
using DotNetty.Transport.Channels;
using Game;
using Game.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Realm;
using Realm.Events;

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

    // WoWChat
    services.AddTransient<IWoWChat, WoWChat>();

    // Dotnetty
    services.AddTransient<IEventLoopGroup, MultithreadEventLoopGroup>();

    // Common
    services.AddTransient<IdleStateCallback>();

    // Realm
    services.AddTransient<RealmChannelInitializer>();

    services.AddTransient<RealmPacketHandler>();

    services.AddTransient<RealmPacketDecoder>();
    services.AddTransient<RealmPacketEncoder>();

    services.AddTransient<RealmConnector>();

    // Game
    // Scoped GameHeaderCrypt instances ensure that decoders and encoders
    // recieve the same crypt instance dependency per scope
    services.AddScoped<GameHeaderCrypt>();
    services.AddScoped<GameHeaderCryptTBC>();
    services.AddScoped<GameHeaderCryptWotLK>();
    services.AddScoped<GameHeaderCryptMoP>();

    // Add a scoped name lookup to allow for shared name caching per scope.
    services.AddScoped<GameNameLookup>();

    services.AddTransient<GamePacketDecoder>();
    services.AddTransient<GamePacketDecoderWotLK>();
    services.AddTransient<GamePacketDecoderCataclysm>();
    services.AddTransient<GamePacketDecoderMoP>();

    services.AddTransient<GamePacketEncoder>();
    services.AddTransient<GamePacketEncoderCataclysm>();
    services.AddTransient<GamePacketEncoderMoP>();

    services.AddTransient<GamePacketHandler>();

    services.AddTransient<GameChannelInitializer>();
    services.AddTransient<GameConnector>();

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

    foreach (var realmPacketHandlerType in realmPacketHandlerTypes)
    {
      services.AddTransient(realmPacketHandlerInterface, realmPacketHandlerType);
      services.AddTransient(realmPacketHandlerType);
    }

    // Game
    var gamePacketHandlerInterface = typeof(IPacketHandler<GameEvent>);
    var gamePacketHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(s => s.GetTypes())
      .Where(p => gamePacketHandlerInterface.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

    foreach (var gamePacketHandlerType in gamePacketHandlerTypes)
    {
      services.AddTransient(gamePacketHandlerInterface, gamePacketHandlerType);
      services.AddTransient(gamePacketHandlerType);
    }

    // Add Known Packet Commands
    var packetCommandAttributeType = typeof(PacketCommandAttribute);

    // Realm
    var realmPacketCommandInterface = typeof(IPacketCommand<RealmEvent>);
    var realmPacketCommandTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(s => s.GetTypes())
      .Where(p => realmPacketCommandInterface.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetCommandAttributeType));

    foreach (var realmPacketCommandType in realmPacketCommandTypes)
    {
      services.AddTransient(realmPacketCommandInterface, realmPacketCommandType);
      services.AddTransient(realmPacketCommandType);
    }

    // Game
    var gamePacketCommandInterface = typeof(IPacketCommand<GameEvent>);
    var gamePacketCommandTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(s => s.GetTypes())
      .Where(p => gamePacketCommandInterface.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetCommandAttributeType));

    foreach (var gamePacketCommandType in gamePacketCommandTypes)
    {
      services.AddTransient(gamePacketCommandInterface, gamePacketCommandType);
      services.AddTransient(gamePacketCommandType);
    }
  }
}