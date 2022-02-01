namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Security.Cryptography;
using System.Text;

[PacketHandler(WorldCommand.SMSG_AUTH_CHALLENGE, WoWExpansion.Vanilla)]
public class ServerAuthChallengePacketHandler : IPacketHandler<GameEvent>
{
  protected readonly byte[] _addonInfo = new byte[]
    {
      0x56, 0x01, 0x00, 0x00, 0x78, 0x9C, 0x75, 0xCC, 0xBD, 0x0E, 0xC2, 0x30, 0x0C, 0x04, 0xE0, 0xF2,
      0x1E, 0xBC, 0x0C, 0x61, 0x40, 0x95, 0xC8, 0x42, 0xC3, 0x8C, 0x4C, 0xE2, 0x22, 0x0B, 0xC7, 0xA9,
      0x8C, 0xCB, 0x4F, 0x9F, 0x1E, 0x16, 0x24, 0x06, 0x73, 0xEB, 0x77, 0x77, 0x81, 0x69, 0x59, 0x40,
      0xCB, 0x69, 0x33, 0x67, 0xA3, 0x26, 0xC7, 0xBE, 0x5B, 0xD5, 0xC7, 0x7A, 0xDF, 0x7D, 0x12, 0xBE,
      0x16, 0xC0, 0x8C, 0x71, 0x24, 0xE4, 0x12, 0x49, 0xA8, 0xC2, 0xE4, 0x95, 0x48, 0x0A, 0xC9, 0xC5,
      0x3D, 0xD8, 0xB6, 0x7A, 0x06, 0x4B, 0xF8, 0x34, 0x0F, 0x15, 0x46, 0x73, 0x67, 0xBB, 0x38, 0xCC,
      0x7A, 0xC7, 0x97, 0x8B, 0xBD, 0xDC, 0x26, 0xCC, 0xFE, 0x30, 0x42, 0xD6, 0xE6, 0xCA, 0x01, 0xA8,
      0xB8, 0x90, 0x80, 0x51, 0xFC, 0xB7, 0xA4, 0x50, 0x70, 0xB8, 0x12, 0xF3, 0x3F, 0x26, 0x41, 0xFD,
      0xB5, 0x37, 0x90, 0x19, 0x66, 0x8F
    };

  protected readonly GameHeaderCrypt _headerCrypt;
  protected readonly WowChatOptions _options;
  protected readonly ILogger<ServerAuthChallengePacketHandler> _logger;

  public ServerAuthChallengePacketHandler(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver headerCryptResolver, ILogger<ServerAuthChallengePacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _headerCrypt = headerCryptResolver(_options.WoW.GetExpansion());
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public GameServerInfo? Realm { get; set; }

  public byte[]? SessionKey { get; set; }

  public int? ClientSeed { get; set; }

  public int? ServerSeed { get; set; }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    if (SessionKey == null)
    {
      throw new InvalidOperationException("SessionKey has not been set.");
    }

    var byteBuf = ParseServerAuthChallenge(ctx, msg);

    _headerCrypt.Init(SessionKey);

    ctx.WriteAndFlushAsync(new Packet(WorldCommand.CMSG_AUTH_CHALLENGE, byteBuf));
  }

  protected virtual IByteBuffer ParseServerAuthChallenge(IChannelHandlerContext ctx, Packet msg)
  {
    if (SessionKey == null)
    {
      throw new InvalidOperationException("SessionKey has not been set.");
    }

    var account = _options.WoW.AccountName.ToUpperInvariant();

    ServerSeed = msg.ByteBuf.ReadInt();

    if (ClientSeed.HasValue == false || ClientSeed.Value == default)
    {
      var rand = RandomNumberGenerator.Create();
      var randomClientSeed = new byte[4];
      rand.GetBytes(randomClientSeed);
      ClientSeed = BitConverter.ToInt32(randomClientSeed);
    }

    var output = ctx.Allocator.Buffer(200, 400);
    output.WriteShortLE(0);
    output.WriteIntLE(_options.WoW.GetBuild());
    output.WriteIntLE(0);
    output.WriteAscii(account);
    output.WriteByte(0);
    output.WriteInt(ClientSeed.Value);

    using SHA1 alg = SHA1.Create();
    var hash = alg.ComputeHash(
      Encoding.ASCII.GetBytes(account).Combine(
        new byte[] { 0, 0, 0, 0 },
        ClientSeed.Value.ToBytes(),
        ServerSeed.Value.ToBytes(),
        SessionKey
        ));
    output.WriteBytes(hash);

    output.WriteBytes(_addonInfo);
    return output;
  }
}