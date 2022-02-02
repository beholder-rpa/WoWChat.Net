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

  public partial class GamePacketHandler : ChannelHandlerAdapter, IObservable<GameEvent>
  {
    protected readonly WowChatOptions _options;
    protected readonly GameHeaderCrypt _headerCrypt;
    protected readonly ILogger<GamePacketHandler> _logger;

    protected IDictionary<int, IPacketHandler<GameEvent>> _packetHandlers = new Dictionary<int, IPacketHandler<GameEvent>>();
    protected IChannelHandlerContext? _context;

    public GamePacketHandler(
      IServiceProvider serviceProvider,
      IOptionsSnapshot<WowChatOptions> options,
      GameHeaderCryptResolver headerCryptResolver,
      ILogger<GamePacketHandler> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _headerCrypt = headerCryptResolver(_options.WoW.GetExpansion());
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (string.IsNullOrWhiteSpace(_options.WoW.AccountName))
      {
        throw new InvalidOperationException("An account name must be specified in configuration");
      }

      //Initialize the packet handlers
      InitPacketHandlers(serviceProvider, _options.WoW.GetExpansion());
    }

    public GameServerInfo? Realm { get; set; } = null;

    public byte[]? SessionKey { get; set; } = null;

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      _context = null;
      OnGameEvent(new GameDisconnectedEvent());
      base.ChannelInactive(context);
    }

    /// <summary>
    /// Binds the game packet handlers defined by the service provider. Called by the constructor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void InitPacketHandlers(IServiceProvider serviceProvider, WoWExpansion expansion)
    {
      // Init PacketHandlers
      var packetHandlerType = typeof(IPacketHandler<GameEvent>);
      var packetHandlerAttributeType = typeof(PacketHandlerAttribute);
      var handlerTypes = serviceProvider.GetServices(packetHandlerType)
        .Select(o => o?.GetType())
        .Where(p => packetHandlerType.IsAssignableFrom(p) && p.CustomAttributes.Any(a => a.AttributeType == packetHandlerAttributeType));

      var packetHandlerGameEventCallback = new Action<GameEvent>((gameEvent) => OnGameEvent(gameEvent));

      foreach (var handlerType in handlerTypes)
      {
        if (handlerType == default) continue;

        var packetHandlerAttributes = handlerType.GetCustomAttributes(false).OfType<PacketHandlerAttribute>();

        foreach (var packetHandlerAttribute in packetHandlerAttributes)
        {
          if ((packetHandlerAttribute.Expansion & expansion) != expansion)
          {
            _logger.LogDebug($"Skipping {handlerType} for {expansion} (indicates {packetHandlerAttribute.Expansion})");
            continue;
          }

          if (_packetHandlers.ContainsKey(packetHandlerAttribute.Id))
          {
            throw new InvalidOperationException($"A packet handler is already registered for packet id {packetHandlerAttribute.Id} ({BitConverter.ToString(packetHandlerAttribute.Id.ToBytes())}), expansion: {packetHandlerAttribute.Expansion}");
          }

          var packetHandler = (IPacketHandler<GameEvent>)serviceProvider.GetRequiredService(handlerType);
          packetHandler.EventCallback = packetHandlerGameEventCallback;

          _packetHandlers.Add(packetHandlerAttribute.Id, packetHandler);
        }
      }
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
      switch (msg.Id)
      {
        case WorldCommand.SMSG_AUTH_CHALLENGE:
          if (_packetHandlers[msg.Id] is not ServerAuthChallengePacketHandler authChallengePacketHandler)
          {
            throw new InvalidOperationException($"Unable to locate ServerAuthChallengePacketHandler for {msg.Id}");
          }
          authChallengePacketHandler.Realm = Realm;
          authChallengePacketHandler.SessionKey = SessionKey;
          authChallengePacketHandler.HandlePacket(context, msg);
          break;
        default:
          if (_packetHandlers.ContainsKey(msg.Id))
          {
            _packetHandlers[msg.Id].HandlePacket(context, msg);
          }
          else if (IgnoredOpcodes.Contains(msg.Id))
          {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
              _logger.LogTrace("Ignored command {id}.", BitConverter.ToString(msg.Id.ToBytes()));
            }
          }
          else
          {
            _logger.LogWarning("A packet handler for command {id} for expansion {expansion} could not be located.", BitConverter.ToString(msg.Id.ToBytes()), _options.WoW.GetExpansion());
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