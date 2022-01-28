namespace WoWChat.Net.Realm
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

  public class RealmPacketHandler : ChannelHandlerAdapter, IObservable<RealmEvent>
  {
    protected readonly WowChatOptions _options;
    protected readonly ILogger<RealmPacketHandler> _logger;

    private int _logonState = 0;
    private SRPClient? _srpClient;

    private bool _isExpectedDisconnect = false;

    private IDictionary<(int, Platform), byte[]> _buildCrcHashes = new Dictionary<(int, Platform), byte[]> {
      { (5875, Platform.Mac), new byte[] { 0x8D, 0x17, 0x3C, 0xC3, 0x81, 0x96, 0x1E, 0xEB, 0xAB, 0xF3, 0x36, 0xF5, 0xE6, 0x67, 0x5B, 0x10, 0x1B, 0xB5, 0x13, 0xE5 } },
      { (5875, Platform.Windows), new byte[] { 0x95, 0xED, 0xB2, 0x7C, 0x78, 0x23, 0xB3, 0x63, 0xCB, 0xDD, 0xAB, 0x56, 0xA3, 0x92, 0xE7, 0xCB, 0x73, 0xFC, 0xCA, 0x20 } },
      { (8606, Platform.Mac), new byte[] { 0xD8, 0xB0, 0xEC, 0xFE, 0x53, 0x4B, 0xC1, 0x13, 0x1E, 0x19, 0xBA, 0xD1, 0xD4, 0xC0, 0xE8, 0x13, 0xEE, 0xE4, 0x99, 0x4F } },
      { (8606, Platform.Windows), new byte[] { 0x31, 0x9A, 0xFA, 0xA3, 0xF2, 0x55, 0x96, 0x82, 0xF9, 0xFF, 0x65, 0x8B, 0xE0, 0x14, 0x56, 0x25, 0x5F, 0x45, 0x6F, 0xB1 } },
      { (12340, Platform.Mac), new byte[] { 0xB7, 0x06, 0xD1, 0x3F, 0xF2, 0xF4, 0x01, 0x88, 0x39, 0x72, 0x94, 0x61, 0xE3, 0xF8, 0xA0, 0xE2, 0xB5, 0xFD, 0xC0, 0x34 } },
      { (12340, Platform.Windows), new byte[] { 0xCD, 0xCB, 0xBD, 0x51, 0x88, 0x31, 0x5E, 0x6B, 0x4D, 0x19, 0x44, 0x9D, 0x49, 0x2D, 0xBC, 0xFA, 0xF1, 0x56, 0xA3, 0x47 } }
    };

    public RealmPacketHandler(IOptions<WowChatOptions> options, ILogger<RealmPacketHandler> logger)
    {
      _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (string.IsNullOrWhiteSpace(_options.AccountName))
      {
        throw new InvalidOperationException("An account name must be specified in configuration");
      }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      if (_isExpectedDisconnect == false)
      { 
        OnRealmEvent(new RealmDisconnectedEvent()
        {
          IsExpected = false,
        });
      }

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
      if (message is Packet packet)
      {
        switch (packet.Id)
        {
          case (int)RealmAuthCommand.CMD_AUTH_LOGON_CHALLENGE when _logonState == 0:
            HandleAuthLogonChallenge(context, packet);
            break;
          case (int)RealmAuthCommand.CMD_AUTH_LOGON_PROOF when _logonState == 1:
            HandleAuthLogonProof(context, packet);
            break;
          case (int)RealmAuthCommand.CMD_REALM_LIST when _logonState == 2:
            HandleRealmList(context, packet);
            break;
          default:
            _logger.LogDebug("Received packet {commandId} in unexpected logonState {logonState}", packet.Id, _logonState);
            OnRealmEvent(new RealmErrorEvent()
            {
              Message = $"Received packet {packet.Id} in unexpected logonState {_logonState}"
            });
            packet.ByteBuf.Release();
            return;
        }
        packet.ByteBuf.Release();
        _logonState += 1;
        return;
      }

      _logger.LogError("Packet is instance of {messageType}", message.GetType());
      base.ChannelRead(context, message);
    }

    protected virtual Packet CreateClientAuthChallenge(IChannelHandlerContext context)
    {
      var username = _options.AccountName;
      var version = _options.Version.Split(".").Select(v => byte.Parse(v)).ToArray();
      var platform = _options.Platform == Platform.Windows ? "Win" : "Mac";

      var byteBuf = context.Allocator.Buffer(50, 100);

      // seems to be 3 for vanilla and 8 for bc/wotlk
      if (_options.GetExpansion() == WoWExpansion.Vanilla)
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
      byteBuf.WriteUnsignedShortLE(_options.GetBuild());
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
      return new Packet((byte)RealmAuthCommand.CMD_AUTH_LOGON_CHALLENGE, byteBuf);
    }

    protected virtual void HandleAuthLogonChallenge(IChannelHandlerContext ctx, Packet msg)
    {
      var _ = msg.ByteBuf.ReadByte(); // error?
      var result = msg.ByteBuf.ReadByte();
      if (!RealmHelpers.IsAuthResultSuccess(result))
      {
        var realmMessage = RealmHelpers.GetMessage(result);
        _logger.LogError("Error Message: {msg}", realmMessage);
        OnRealmEvent(new RealmErrorEvent()
        {
          Message = $"Error Message: {realmMessage}"
        });
        ctx.CloseAsync().Wait();
        OnRealmEvent(new RealmErrorEvent()
        {
          Message = realmMessage,
        });
        return;
      }

      var B = msg.ByteBuf.ReadBytes(32).GetArrayCopy();
      var gLength = msg.ByteBuf.ReadByte();
      var g = msg.ByteBuf.ReadBytes(gLength).GetArrayCopy();
      var nLength = msg.ByteBuf.ReadByte();
      var n = msg.ByteBuf.ReadBytes(nLength).GetArrayCopy();
      var salt = msg.ByteBuf.ReadBytes(32).GetArrayCopy();
      var nonce = msg.ByteBuf.ReadBytes(16).GetArrayCopy();
      var securityFlag = msg.ByteBuf.ReadByte();
      if (securityFlag != 0)
      {
        _logger.LogDebug("Recieved non-zero security flag: {securityFlag}", securityFlag);
        OnRealmEvent(new RealmErrorEvent()
        {
          Message = $"Two factor authentication is enabled for this account. Please disable it or use another account."
        });
        ctx.CloseAsync().Wait();
        return;
      }

      _srpClient = new SRPClient(
        _options.AccountName,
        _options.AccountPassword,
        B,
        g,
        n,
        salt,
        nonce
        );
      
      var aArray = _srpClient.A.ToCleanByteArray();

      var byteBuf = ctx.Allocator.Buffer(74, 74);
      byteBuf.WriteBytes(aArray);
      byteBuf.WriteBytes(_srpClient.Proof);
      var crc = new byte[20];
      if (_buildCrcHashes.ContainsKey((_options.GetBuild(), _options.Platform)))
      {
        crc = _buildCrcHashes[(_options.GetBuild(), _options.Platform)];
      }
      byteBuf.WriteBytes(crc);
      byteBuf.WriteByte(0);
      byteBuf.WriteByte(0);

      ctx.WriteAndFlushAsync(new Packet((byte)RealmAuthCommand.CMD_AUTH_LOGON_PROOF, byteBuf));
    }

    protected virtual void HandleAuthLogonProof(IChannelHandlerContext ctx, Packet packet)
    {
      var result = packet.ByteBuf.ReadByte();

      if (!RealmHelpers.IsAuthResultSuccess(result))
      {
        var realmMessage = RealmHelpers.GetMessage(result);
        _logger.LogError("Error Message: {msg}", realmMessage);
        _isExpectedDisconnect = true;
        ctx.CloseAsync().Wait();

        if (result == (byte)RealmAuthResult.WOW_FAIL_UNKNOWN_ACCOUNT)
        {
          // seems sometimes this error happens even on a legit connect. so just run regular reconnect loop
          OnRealmEvent(new RealmDisconnectedEvent()
          {
            IsExpected = true,
            Reason = realmMessage
          });
        }
        else
        {
          OnRealmEvent(new RealmErrorEvent()
          {
            Message = realmMessage,
          });
        }
        return;
      }

      var proof = packet.ByteBuf.ReadBytes(20).GetArrayCopy();
      if (_srpClient == null)
      {
        throw new InvalidOperationException("SRPClient was null when handling login proof");
      }

      if (!_srpClient.ExpectedProofResponse.SequenceEqual(proof))
      {
        _logger.LogError("Logon proof generated by client and server differ. Something is very wrong! Will try to reconnect in a moment.");
        _isExpectedDisconnect = true;
        ctx.CloseAsync().Wait();
        // Also sometimes happens on a legit connect.
        OnRealmEvent(new RealmDisconnectedEvent()
        {
          IsExpected = true,
          Reason = "Logon proof generated by client and server differ."
        });
        return;
      }

      var accountFlag = packet.ByteBuf.ReadIntLE(); // account flag

      OnRealmEvent(new RealmAuthenticatedEvent()
      {
        SessionKey = _srpClient.SessionKey,
        AccountFlag = accountFlag
      });

      // ask for realm list
      var ret = ctx.Allocator.Buffer(4, 4);
      ret.WriteIntLE(0);
      ctx.WriteAndFlushAsync(new Packet((byte)RealmAuthCommand.CMD_REALM_LIST, ret));
    }

    protected virtual void HandleRealmList(IChannelHandlerContext ctx, Packet packet)
    {
      var realmList = ParseRealmList(packet);
      OnRealmEvent(new RealmListEvent()
      {
        RealmList = realmList
      });

      _isExpectedDisconnect = true;
      ctx.CloseAsync().Wait();
      OnRealmEvent(new RealmDisconnectedEvent()
      {
        AutoReconnect = false,
        IsExpected = true,
        Reason = "Realm List Retrieved"
      });
    }

    protected virtual IList<Realm> ParseRealmList(Packet packet)
    {
      var result = new List<Realm>();
      packet.ByteBuf.ReadIntLE(); // unknown
      var numRealms = packet.ByteBuf.ReadByte();
      for(int i = 0; i < numRealms; i++)
      {
        var realmType = packet.ByteBuf.ReadByte(); // realm type (pvp/pve)
        var realmLocked = packet.ByteBuf.ReadByte(); // Locked/Unlocked
        var realmFlags = packet.ByteBuf.ReadByte(); // realm flags (offline/recommended/for newbs)

        // On Vanilla MaNGOS, there is some string manipulation to insert the build information into the name itself
        // if realm flags specify to do so. But that is counter-intuitive to matching the config, so let's remove it.
        var name = (realmFlags & 0x04) == 0x04
          ? Regex.Replace(packet.ByteBuf.ReadString(), " \\(\\d+,\\d+,\\d+\\)", "")
          : packet.ByteBuf.ReadString();

        var address = packet.ByteBuf.ReadString();
        var population = packet.ByteBuf.ReadUnsignedInt(); // population
        var characters = packet.ByteBuf.ReadByte(); // num of characters
        var timeZone = packet.ByteBuf.ReadByte(); // timezone
        var realmId = packet.ByteBuf.ReadByte();

        var addressTokens = address.Split(':');
        var host = addressTokens[0];
        // some servers "overflow" the port on purpose to dissuade rudimentary bots
        var port = addressTokens.Length > 1 ? int.Parse(addressTokens[1]) & 0xFFFF : 8085;

        result.Add(new Realm()
        {
          Type = realmType,
          Locked = realmLocked,
          Flags = realmFlags,
          Name = name,
          Host = host,
          Port = port,
          Population = population,
          Characters = characters,
          TimeZone = timeZone,
          RealmId = realmId,
        }); ;
      }

      return result;
    }

    #region IObservable<RealmEvent>
    private readonly ConcurrentDictionary<IObserver<RealmEvent>, RealmEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<RealmEvent>, RealmEventUnsubscriber>();

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
