namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Transport.Channels;
  using Events;
  using Extensions;
  using Game.PacketHandlers;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Concurrent;
  using System.Timers;

  public partial class GamePacketHandler : ChannelHandlerAdapter, IObservable<GameEvent>
  {
    protected readonly WowChatOptions _options;
    protected readonly GameHeaderCrypt _headerCrypt;
    protected readonly ILogger<GamePacketHandler> _logger;
    protected readonly Timer _pingTimer;
    protected readonly Random _random = new Random();

    protected int _pingId = 0;
    protected IDictionary<(int id, WoWExpansion? expansion), IPacketHandler> _packetHandlers = new Dictionary<(int id, WoWExpansion? expansion), IPacketHandler>();
    protected IChannelHandlerContext? _context;

    public GamePacketHandler(
      IServiceProvider serviceProvider,
      IOptionsSnapshot<WowChatOptions> options,
      GameHeaderCryptResolver headerCryptResolver,
      ILogger<GamePacketHandler> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _headerCrypt = headerCryptResolver(_options.GetExpansion());
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (string.IsNullOrWhiteSpace(_options.AccountName))
      {
        throw new InvalidOperationException("An account name must be specified in configuration");
      }

      // Init PacketHandlers
      var packetHandlerType = typeof(IPacketHandler);
      var packetHandlerAttributeType = typeof(PacketHandlerAttribute);
      var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Where(p => packetHandlerType.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

      var packetHandlerGameEventCallback = new Action<GameEvent>((gameEvent) => OnGameEvent(gameEvent));

      foreach(var handlerType in handlerTypes)
      {
        var packetHandlerAttributes = handlerType.GetCustomAttributes(false).OfType<PacketHandlerAttribute>();
        var packetHandler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        packetHandler.GameEventCallback = packetHandlerGameEventCallback;

        foreach (var packetHandlerAttribute in packetHandlerAttributes)
        {
          if (_packetHandlers.ContainsKey((packetHandlerAttribute.Id, packetHandlerAttribute.Expansion)))
          {
            throw new InvalidOperationException($"A packet handler is already registered for packet id {packetHandlerAttribute.Id} ({BitConverter.ToString(packetHandlerAttribute.Id.ToBytes())}), expansion: {packetHandlerAttribute.Expansion}");
          }
          _packetHandlers.Add((packetHandlerAttribute.Id, packetHandlerAttribute.Expansion), packetHandler);
        }
      }

      _pingTimer = new Timer(30)
      {
        AutoReset = true,
        Enabled = false,
      };
      _pingTimer.Elapsed += RunPingExecutor;
    }

    public GameRealm? Realm { get; set; } = null;

    public byte[]? SessionKey { get; set; } = null;

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      _context = null;
      OnGameEvent(new GameDisconnectedEvent());
      base.ChannelInactive(context);
    }

    // Vanilla does not have a keep alive packet
    protected virtual void RunKeepAliveExecutor()
    {
    }

    protected virtual void RunPingExecutor(object? sender, ElapsedEventArgs e)
    {
      if (_context == null || !_context.Channel.Active)
      {
        _logger.LogDebug("Attempted to ping while the context/channel is not open. Stopping ping timer.");
        _pingTimer.Enabled = false;
        return;
      }

      var latency = _random.Next(50) + 90;

      var byteBuf = _context.Allocator.Buffer(8, 8);
      byteBuf.WriteIntLE(_pingId);
      byteBuf.WriteIntLE(latency);

      _context.WriteAndFlushAsync(new Packet(WorldCommand.CMSG_PING, byteBuf)).Wait();
      _pingId += 1;
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
      _logger.LogDebug("Game Channel Active");
      _context = context;

      //base.ChannelActive(context);
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

    protected virtual void ChannelParse(IChannelHandlerContext context, Packet msg)
    {
      var key = (msg.Id, _options.GetExpansion());
      var genericKey = (msg.Id, WoWExpansion.All);

      switch (msg.Id)
      {
        case WorldCommand.SMSG_AUTH_CHALLENGE:
          if (_packetHandlers[key] is not ServerAuthChallengePacketHandler authChallengePacketHandler)
          {
            throw new InvalidOperationException($"Unable to locate ServerAuthChallengePacketHandler for {key.Id}");
          }
          authChallengePacketHandler.Realm = Realm;
          authChallengePacketHandler.SessionKey = SessionKey;
          authChallengePacketHandler.HandlePacket(context, msg);
          break;
        default:
          if (_packetHandlers.ContainsKey(key))
          {
            _packetHandlers[key].HandlePacket(context, msg);
          }
          else if (_packetHandlers.ContainsKey(genericKey))
          {
            _packetHandlers[genericKey].HandlePacket(context, msg);
          }
          else
          {
            _logger.LogWarning("A packet handler for command {id} for expansion {expansion} could not be located.", BitConverter.ToString(msg.Id.ToBytes()), key.Item2.ToString());
          }
          break;
      }
    }

    public void SendLogout()
    {
      if (_context == null || !_context.Channel.Active)
      {
        return;
      }

      _context.WriteAndFlushAsync(new Packet(WorldCommand.CMSG_LOGOUT_REQUEST)).Wait();
    }

    #region IObservable<GameEvent>
    private readonly ConcurrentDictionary<IObserver<GameEvent>, GameEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<GameEvent>, GameEventUnsubscriber>();

    IDisposable IObservable<GameEvent>.Subscribe(IObserver<GameEvent> observer)
    {
      return _observers.GetOrAdd(observer, new GameEventUnsubscriber(this, observer));
    }

    /// <summary>
    /// Produces Game Events
    /// </summary>
    /// <param name="gameEvent"></param>
    private void OnGameEvent(GameEvent gameEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(gameEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    private sealed class GameEventUnsubscriber : IDisposable
    {
      private readonly GamePacketHandler _parent;
      private readonly IObserver<GameEvent> _observer;

      public GameEventUnsubscriber(GamePacketHandler parent, IObserver<GameEvent> observer)
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
