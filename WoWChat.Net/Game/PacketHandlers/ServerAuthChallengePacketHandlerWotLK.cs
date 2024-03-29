﻿namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Extensions;
using global::WoWChat.Net.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

[PacketHandler(WorldCommand.SMSG_AUTH_CHALLENGE, WoWExpansion.WotLK)]
public class ServerAuthChallengePacketHandlerWotLK : ServerAuthChallengePacketHandlerTBC
{
  protected new byte[] _addonInfo = new byte[]
    {
      0x9E, 0x02, 0x00, 0x00, 0x78, 0x9C, 0x75, 0xD2, 0xC1, 0x6A, 0xC3, 0x30, 0x0C, 0xC6, 0x71, 0xEF,
      0x29, 0x76, 0xE9, 0x9B, 0xEC, 0xB4, 0xB4, 0x50, 0xC2, 0xEA, 0xCB, 0xE2, 0x9E, 0x8B, 0x62, 0x7F,
      0x4B, 0x44, 0x6C, 0x39, 0x38, 0x4E, 0xB7, 0xF6, 0x3D, 0xFA, 0xBE, 0x65, 0xB7, 0x0D, 0x94, 0xF3,
      0x4F, 0x48, 0xF0, 0x47, 0xAF, 0xC6, 0x98, 0x26, 0xF2, 0xFD, 0x4E, 0x25, 0x5C, 0xDE, 0xFD, 0xC8,
      0xB8, 0x22, 0x41, 0xEA, 0xB9, 0x35, 0x2F, 0xE9, 0x7B, 0x77, 0x32, 0xFF, 0xBC, 0x40, 0x48, 0x97,
      0xD5, 0x57, 0xCE, 0xA2, 0x5A, 0x43, 0xA5, 0x47, 0x59, 0xC6, 0x3C, 0x6F, 0x70, 0xAD, 0x11, 0x5F,
      0x8C, 0x18, 0x2C, 0x0B, 0x27, 0x9A, 0xB5, 0x21, 0x96, 0xC0, 0x32, 0xA8, 0x0B, 0xF6, 0x14, 0x21,
      0x81, 0x8A, 0x46, 0x39, 0xF5, 0x54, 0x4F, 0x79, 0xD8, 0x34, 0x87, 0x9F, 0xAA, 0xE0, 0x01, 0xFD,
      0x3A, 0xB8, 0x9C, 0xE3, 0xA2, 0xE0, 0xD1, 0xEE, 0x47, 0xD2, 0x0B, 0x1D, 0x6D, 0xB7, 0x96, 0x2B,
      0x6E, 0x3A, 0xC6, 0xDB, 0x3C, 0xEA, 0xB2, 0x72, 0x0C, 0x0D, 0xC9, 0xA4, 0x6A, 0x2B, 0xCB, 0x0C,
      0xAF, 0x1F, 0x6C, 0x2B, 0x52, 0x97, 0xFD, 0x84, 0xBA, 0x95, 0xC7, 0x92, 0x2F, 0x59, 0x95, 0x4F,
      0xE2, 0xA0, 0x82, 0xFB, 0x2D, 0xAA, 0xDF, 0x73, 0x9C, 0x60, 0x49, 0x68, 0x80, 0xD6, 0xDB, 0xE5,
      0x09, 0xFA, 0x13, 0xB8, 0x42, 0x01, 0xDD, 0xC4, 0x31, 0x6E, 0x31, 0x0B, 0xCA, 0x5F, 0x7B, 0x7B,
      0x1C, 0x3E, 0x9E, 0xE1, 0x93, 0xC8, 0x8D
    };

  public ServerAuthChallengePacketHandlerWotLK(IOptionsSnapshot<WowChatOptions> options, GameHeaderCryptResolver headerCryptResolver, ILogger<ServerAuthChallengePacketHandler> logger)
    : base(options, headerCryptResolver, logger)
  {
  }

  protected override IByteBuffer ParseServerAuthChallenge(IChannelHandlerContext ctx, Packet msg)
  {
    if (Realm == null)
    {
      throw new InvalidOperationException("Realm has not been set.");
    }

    if (Session == null)
    {
      throw new InvalidOperationException("Session has not been set.");
    }

    if (DateTime.UtcNow.Subtract(Session.StartTime) > TimeSpan.FromSeconds(60))
    {
      throw new InvalidOperationException("Session was initialized over a minute ago - aborting.");
    }

    var account = _options.WoW.AccountName.ToUpperInvariant();

    msg.ByteBuf.SkipBytes(4); // wotlk
    ServerSeed = msg.ByteBuf.ReadInt();

    if (Session.ClientSeed.HasValue == false || Session.ClientSeed.Value == default)
    {
      var rand = RandomNumberGenerator.Create();
      var randomClientSeed = new byte[4];
      rand.GetBytes(randomClientSeed);
      Session = Session with { ClientSeed = BitConverter.ToInt32(randomClientSeed) };
    }

    var output = ctx.Allocator.Buffer(200, 400);
    output.WriteShortLE(0);
    output.WriteIntLE(_options.WoW.GetBuild());
    output.WriteIntLE(0);
    output.WriteAscii(account);
    output.WriteByte(0);
    output.WriteInt(0); // wotlk
    output.WriteInt(Session.ClientSeed.Value);
    output.WriteIntLE(0); // wotlk
    output.WriteIntLE(0); // wotlk
    output.WriteIntLE(Realm.RealmId); // wotlk
    output.WriteLongLE(3); // wotlk

    using SHA1 alg = SHA1.Create();
    var hash = alg.ComputeHash(
      Encoding.ASCII.GetBytes(account).Combine(
        new byte[] { 0, 0, 0, 0 },
        Session.ClientSeed.Value.ToBytes(),
        ServerSeed.Value.ToBytes(),
        Session.SessionKey
        ));
    output.WriteBytes(hash);

    output.WriteBytes(_addonInfo);
    return output;
  }
}