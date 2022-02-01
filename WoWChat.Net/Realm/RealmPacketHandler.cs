namespace WoWChat.Net.Realm
{
  using Common;
  using DotNetty.Transport.Channels;
  using Events;
  using Extensions;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using Realm.PacketHandlers;
  using System;
  using System.Collections.Concurrent;
  using System.Linq;
  using System.Threading.Tasks;

  public class RealmPacketHandler : ChannelHandlerAdapter, IObservable<RealmEvent>
  {
    protected readonly WowChatOptions _options;
    protected readonly ILogger<RealmPacketHandler> _logger;
    protected IDictionary<int, IPacketHandler<RealmEvent>> _packetHandlers = new Dictionary<int, IPacketHandler<RealmEvent>>();

    private int _logonState = 0;
    private SRPClient? _srpClient;

    public RealmPacketHandler(IServiceProvider serviceProvider, IOptionsSnapshot<WowChatOptions> options, ILogger<RealmPacketHandler> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (string.IsNullOrWhiteSpace(_options.WoW.AccountName))
      {
        throw new InvalidOperationException("An account name must be specified in configuration");
      }

      if (string.IsNullOrWhiteSpace(_options.WoW.AccountPassword))
      {
        throw new InvalidOperationException("An account password must be specified in configuration");
      }

      //Initialize the packet handlers
      InitPacketHandlers(serviceProvider, _options.WoW.GetExpansion());
    }

    /// <summary>
    /// Binds the realm packet handlers defined by the service provider. Called by the constructor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void InitPacketHandlers(IServiceProvider serviceProvider, WoWExpansion expansion)
    {
      // Init PacketHandlers
      var packetHandlerType = typeof(IPacketHandler<RealmEvent>);
      var packetHandlerAttributeType = typeof(PacketHandlerAttribute);
      var handlerTypes = serviceProvider.GetServices(packetHandlerType)
        .Select(o => o?.GetType())
        .Where(p => p != null && packetHandlerType.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

      var packetHandlerGameEventCallback = new Action<RealmEvent>((gameEvent) => OnRealmEvent(gameEvent));

      foreach (var handlerType in handlerTypes)
      {
        if (handlerType == default) continue;

        var packetHandlerAttributes = handlerType.GetCustomAttributes(false).OfType<PacketHandlerAttribute>();

        foreach (var packetHandlerAttribute in packetHandlerAttributes)
        {
          if ((packetHandlerAttribute.Expansion & expansion) != expansion)
          {
            _logger.LogInformation($"Skipping {handlerType} for {expansion} (indicates {packetHandlerAttribute.Expansion})");
            continue;
          }

          if (_packetHandlers.ContainsKey(packetHandlerAttribute.Id))
          {
            throw new InvalidOperationException($"A packet handler is already registered for packet id {packetHandlerAttribute.Id} ({BitConverter.ToString(packetHandlerAttribute.Id.ToBytes())}), expansion: {packetHandlerAttribute.Expansion}");
          }

          var packetHandler = (IPacketHandler<RealmEvent>)serviceProvider.GetRequiredService(handlerType);
          packetHandler.EventCallback = packetHandlerGameEventCallback;

          _packetHandlers.Add(packetHandlerAttribute.Id, packetHandler);
        }
      }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      OnRealmEvent(new RealmDisconnectedEvent());

      base.ChannelInactive(context);
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
      _logger.LogDebug("Realm Channel Active");
      var authChallenge = CreateClientAuthChallenge(context);

      context.WriteAndFlushAsync(authChallenge).Wait();

      base.ChannelActive(context);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
      if (message is Packet packet == false)
      {
        _logger.LogError("Packet is instance of {messageType}", message.GetType());
        base.ChannelRead(context, message);
        return;
      }

      ChannelParse(context, packet);
    }

    protected virtual void ChannelParse(IChannelHandlerContext ctx, Packet msg)
    {
      switch (msg.Id)
      {
        case RealmCommand.CMD_AUTH_LOGON_CHALLENGE when _logonState == 0:
          if (_packetHandlers[RealmCommand.CMD_AUTH_LOGON_CHALLENGE] is not LogonAuthChallengePacketHandler authChallengePacketHandler)
          {
            throw new InvalidOperationException($"Unable to locate LogonAuthChallengePacketHandler for {RealmCommand.CMD_AUTH_LOGON_CHALLENGE}");
          }
          authChallengePacketHandler.HandlePacket(ctx, msg);
          _srpClient = authChallengePacketHandler.SRPClient;
          break;
        case RealmCommand.CMD_AUTH_LOGON_PROOF when _logonState == 1:
          if (_packetHandlers[RealmCommand.CMD_AUTH_LOGON_PROOF] is not LogonAuthProofPacketHandler authProofPacketHandler)
          {
            throw new InvalidOperationException($"Unable to locate LogonAuthProofPacketHandler for {RealmCommand.CMD_AUTH_LOGON_PROOF}");
          }
          authProofPacketHandler.SRPClient = _srpClient;
          authProofPacketHandler.HandlePacket(ctx, msg);
          break;
        case RealmCommand.CMD_REALM_LIST when _logonState == 2:
          if (_packetHandlers[RealmCommand.CMD_REALM_LIST] is not RealmListPacketHandler realmListPacketHandler)
          {
            throw new InvalidOperationException($"Unable to locate RealmListPacketHandler for {RealmCommand.CMD_REALM_LIST}");
          }
          realmListPacketHandler.HandlePacket(ctx, msg);
          break;
        default:
          _logger.LogDebug("Received packet {commandId} in unexpected logonState {logonState}", msg.Id, _logonState);
          OnRealmEvent(new RealmErrorEvent()
          {
            Message = $"Received packet {msg.Id} in unexpected logonState {_logonState}"
          });
          msg.ByteBuf.Release();
          return;
      }
      msg.ByteBuf.Release();
      _logonState += 1;
    }

    protected virtual Packet CreateClientAuthChallenge(IChannelHandlerContext context)
    {
      var username = _options.WoW.AccountName;
      var version = _options.WoW.Version.Split(".").Select(v => byte.Parse(v)).ToArray();
      var platform = _options.WoW.Platform == Platform.Windows ? "Win" : "Mac";

      var byteBuf = context.Allocator.Buffer(50, 100);

      // seems to be 3 for vanilla and 8 for bc/wotlk
      if (_options.WoW.GetExpansion() == WoWExpansion.Vanilla)
      {
        byteBuf.WriteByte(3);
      }
      else
      {
        byteBuf.WriteByte(8);
      }

      // https://wowdev.wiki/CMD_AUTH_LOGON_CHALLENGE_Client
      byteBuf.WriteUnsignedShortLE((ushort)(username.Length + 30));
      byteBuf.WriteStringLE("WoW");
      byteBuf.WriteBytes(version);
      byteBuf.WriteUnsignedShortLE(_options.WoW.GetBuild());
      byteBuf.WriteStringLE("x86");
      byteBuf.WriteStringLE(platform);
      byteBuf.WriteAsciiLE("enUS");
      byteBuf.WriteUnsignedIntLE(0);
      byteBuf.WriteByte(127);
      byteBuf.WriteByte(0);
      byteBuf.WriteByte(0);
      byteBuf.WriteByte(1);
      byteBuf.WriteByte((byte)username.Length);
      byteBuf.WriteAscii(username.ToUpperInvariant());
      return new Packet(RealmCommand.CMD_AUTH_LOGON_CHALLENGE, byteBuf);
    }

    #region IObservable<RealmEvent>
    private readonly ConcurrentDictionary<IObserver<RealmEvent>, RealmEventUnsubscriber> _observers = new();

    IDisposable IObservable<RealmEvent>.Subscribe(IObserver<RealmEvent> observer)
    {
      return _observers.GetOrAdd(observer, new RealmEventUnsubscriber(this, observer));
    }

    /// <summary>
    /// Produces Realm Events
    /// </summary>
    /// <param name="realmEvent"></param>
    private void OnRealmEvent(RealmEvent realmEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(realmEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    private sealed class RealmEventUnsubscriber : IDisposable
    {
      private readonly RealmPacketHandler _parent;
      private readonly IObserver<RealmEvent> _observer;

      public RealmEventUnsubscriber(RealmPacketHandler parent, IObserver<RealmEvent> observer)
      {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
      }

      public void Dispose()
      {
        if (_observer != null && _parent._observers.ContainsKey(_observer))
        {
          _parent._observers.TryRemove(_observer, out _);
        }
      }
    }
    #endregion
  }
}