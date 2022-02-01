namespace WoWChat.Net.Realm.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Security.Cryptography;

[PacketHandler(RealmCommand.CMD_AUTH_LOGON_CHALLENGE, WoWExpansion.All)]
public class LogonAuthChallengePacketHandler : IPacketHandler<RealmEvent>
{
  private readonly IDictionary<(int, Platform), byte[]> _buildCrcHashes = new Dictionary<(int, Platform), byte[]> {
      { (5875, Platform.Mac), new byte[] { 0x8D, 0x17, 0x3C, 0xC3, 0x81, 0x96, 0x1E, 0xEB, 0xAB, 0xF3, 0x36, 0xF5, 0xE6, 0x67, 0x5B, 0x10, 0x1B, 0xB5, 0x13, 0xE5 } },
      { (5875, Platform.Windows), new byte[] { 0x95, 0xED, 0xB2, 0x7C, 0x78, 0x23, 0xB3, 0x63, 0xCB, 0xDD, 0xAB, 0x56, 0xA3, 0x92, 0xE7, 0xCB, 0x73, 0xFC, 0xCA, 0x20 } },
      { (8606, Platform.Mac), new byte[] { 0xD8, 0xB0, 0xEC, 0xFE, 0x53, 0x4B, 0xC1, 0x13, 0x1E, 0x19, 0xBA, 0xD1, 0xD4, 0xC0, 0xE8, 0x13, 0xEE, 0xE4, 0x99, 0x4F } },
      { (8606, Platform.Windows), new byte[] { 0x31, 0x9A, 0xFA, 0xA3, 0xF2, 0x55, 0x96, 0x82, 0xF9, 0xFF, 0x65, 0x8B, 0xE0, 0x14, 0x56, 0x25, 0x5F, 0x45, 0x6F, 0xB1 } },
      { (12340, Platform.Mac), new byte[] { 0xB7, 0x06, 0xD1, 0x3F, 0xF2, 0xF4, 0x01, 0x88, 0x39, 0x72, 0x94, 0x61, 0xE3, 0xF8, 0xA0, 0xE2, 0xB5, 0xFD, 0xC0, 0x34 } },
      { (12340, Platform.Windows), new byte[] { 0xCD, 0xCB, 0xBD, 0x51, 0x88, 0x31, 0x5E, 0x6B, 0x4D, 0x19, 0x44, 0x9D, 0x49, 0x2D, 0xBC, 0xFA, 0xF1, 0x56, 0xA3, 0x47 } }
    };

  protected readonly WowChatOptions _options;
  protected readonly ILogger<LogonAuthChallengePacketHandler> _logger;

  public LogonAuthChallengePacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<LogonAuthChallengePacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public SRPClient? SRPClient { get; protected set; }

  /// <summary>
  /// Specifies an initial keypair to provide to the SRP Client. Only use this to initialize a SRPClient to match one defined elsewhere or for testing.
  /// </summary>
  public byte[]? InitialKeyPair { get; set; } = null;

  public Action<RealmEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var _ = msg.ByteBuf.ReadByte(); // error?
    var result = msg.ByteBuf.ReadByte();
    if (!RealmHelpers.IsAuthResultSuccess(result))
    {
      var realmMessage = RealmHelpers.GetMessage(result);
      _logger.LogError("Error Message: {msg}", realmMessage);
      EventCallback?.Invoke(new RealmErrorEvent()
      {
        Message = $"Error Message: {realmMessage}"
      });
      EventCallback?.Invoke(new RealmErrorEvent()
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
      _logger.LogDebug("Received non-zero security flag: {securityFlag}", securityFlag);
      EventCallback?.Invoke(new RealmErrorEvent()
      {
        Message = $"Two factor authentication is enabled for this account. Please disable it or use another account."
      });
      return;
    }

    SRPClient = new SRPClient(
    _options.WoW.AccountName,
    _options.WoW.AccountPassword,
    B,
    g,
    n,
    salt,
    nonce,
    InitialKeyPair
    );

    var aArray = SRPClient.A.ToCleanByteArray();

    var byteBuf = ctx.Allocator.Buffer(74, 74);
    byteBuf.WriteBytes(aArray);
    byteBuf.WriteBytes(SRPClient.Proof);
    var crc = new byte[20];
    if (_buildCrcHashes.ContainsKey((_options.WoW.GetBuild(), _options.WoW.Platform)))
    {
      crc = _buildCrcHashes[(_options.WoW.GetBuild(), _options.WoW.Platform)];
    }
    using SHA1 alg = SHA1.Create();
    var crcsha = alg.ComputeHash(aArray.Combine(crc));
    byteBuf.WriteBytes(crcsha);
    byteBuf.WriteByte(0);
    byteBuf.WriteByte(0);

    ctx.WriteAndFlushAsync(new Packet(RealmCommand.CMD_AUTH_LOGON_PROOF, byteBuf));
  }
}
