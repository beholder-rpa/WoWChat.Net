namespace WoWChat.Net.Realm
{
  using DotNetty.Transport.Channels;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using System;
  using System.Linq;
  using Common;
  using Options;

  public class RealmPacketHandler : ChannelHandlerAdapter
  {
    protected readonly WowChatOptions _options;
    protected readonly ILogger<RealmPacketHandler> _logger;

    private int _logonState = 0;

    public RealmPacketHandler(IOptions<WowChatOptions> options, ILogger<RealmPacketHandler> logger)
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
      _logger.LogInformation("Connected! Sending account login information...");
      var authChallenge = CreateClientAuthChallenge();

      context.WriteAndFlushAsync(authChallenge).Wait();

      base.ChannelActive(context);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
      if (message is Packet packet)
      {
        switch (packet.CommandId)
        {
          case (int)RealmAuthCommand.CMD_AUTH_LOGON_CHALLENGE when _logonState == 0:
            break;
          case (int)RealmAuthCommand.CMD_AUTH_LOGON_PROOF when _logonState == 1:
            break;
          case (int)RealmAuthCommand.CMD_REALM_LIST when _logonState == 2:
            break;
          default:
            _logger.LogInformation("Received packet {commandId} in unexpected logonState {logonState}", packet.CommandId, _logonState);
            packet.Dispose();
            return;
        }
        packet.Dispose();
        _logonState += 1;
      }

      _logger.LogError("Packet is instance of {messageType}", message.GetType());
      base.ChannelRead(context, message);
    }

    protected virtual Packet CreateClientAuthChallenge()
    {
      var username = _options.AccountName;
      var version = _options.Version.Split(".").Select(v => byte.Parse(v)).ToArray();
      var platform = _options.Platform == Platform.Windows ? "Win" : "Mac";

      var packet = new Packet((int)RealmAuthCommand.CMD_AUTH_LOGON_CHALLENGE, 128);

      // seems to be 3 for vanilla and 8 for bc/wotlk
      if (_options.GetExpansion() == WoWExpansion.Vanilla)
      {
        packet.WriteByte(3);
      }
      else
      {
        packet.WriteByte(8);
      }

      // https://wowdev.wiki/CMD_AUTH_LOGON_CHALLENGE_Client
      packet.WriteUShortLE((ushort)(username.Length + 30));
      packet.WriteStringLE("WoW");
      packet.WriteBytes(version);
      packet.WriteUShortLE(_options.GetBuild());
      packet.WriteStringLE("x86");
      packet.WriteStringLE(platform);
      packet.WriteAsciiLE("enUS");
      packet.WriteUIntLE(0);
      packet.WriteByte(127);
      packet.WriteByte(0);
      packet.WriteByte(0);
      packet.WriteByte(1);
      packet.WriteByte((byte)username.Length);
      packet.WriteAscii(username.ToUpperInvariant());

      var foo = BitConverter.ToString(packet.ByteBuf);
      return packet;
    }
  }
}
