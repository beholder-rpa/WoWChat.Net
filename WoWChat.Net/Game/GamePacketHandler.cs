namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Transport.Channels;
  using Events;
  using Extensions;
  using Helpers;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Concurrent;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class GamePacketHandler : ChannelHandlerAdapter, IObservable<GameEvent>
  {
    protected readonly WowChatOptions _options;
    protected readonly ILogger<GamePacketHandler> _logger;


    public GamePacketHandler(IOptions<WowChatOptions> options, ILogger<GamePacketHandler> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (string.IsNullOrWhiteSpace(_options.AccountName))
      {
        throw new InvalidOperationException("An account name must be specified in configuration");
      }
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
      _logger.LogDebug("Game Channel Active");

      base.ChannelActive(context);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
      if (message is Packet packet)
      {
        packet.ByteBuf.Release();
        return;
      }

      _logger.LogError("Packet is instance of {messageType}", message.GetType());
      base.ChannelRead(context, message);
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
    private void OnRealmEvent(GameEvent gameEvent)
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
