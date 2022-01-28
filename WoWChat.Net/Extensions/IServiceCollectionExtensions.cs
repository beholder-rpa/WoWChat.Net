namespace Microsoft.Extensions.DependencyInjection;

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Configuration;
using WoWChat.Net;
using WoWChat.Net.Common;
using WoWChat.Net.Options;
using WoWChat.Net.Realm;
using WoWChat.Net.Realm.Events;

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
    if (wowChatOptions.GetExpansion() == WoWExpansion.Vanilla)
    {
      services.AddSingleton<RealmPacketHandler, RealmPacketHandler>();
    }
    else
    {
      services.AddSingleton<RealmPacketHandler, RealmPacketHandlerTBC>();
    }

    services.AddSingleton<RealmPacketDecoder>();
    services.AddSingleton<RealmPacketEncoder>();
    services.AddSingleton<RealmChannelHandler>();
    services.AddSingleton<RealmConnector>();

    // Game
    // TODO...

    // WoWChat
    services.AddSingleton<IWoWChat, WoWChat>();
  }
}