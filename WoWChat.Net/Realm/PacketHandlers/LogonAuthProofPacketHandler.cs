﻿namespace WoWChat.Net.Realm.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

[PacketHandler(RealmCommand.CMD_AUTH_LOGON_PROOF, WoWExpansion.All)]
public class LogonAuthProofPacketHandler : IPacketHandler<RealmEvent>
{
  protected readonly WowChatOptions _options;
  protected readonly ILogger<LogonAuthProofPacketHandler> _logger;

  public LogonAuthProofPacketHandler(IOptionsSnapshot<WowChatOptions> options, ILogger<LogonAuthProofPacketHandler> logger)
  {
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<RealmEvent>? EventCallback { get; set; }

  public SRPClient? SRPClient { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    var result = msg.ByteBuf.ReadByte();

    if (!RealmHelpers.IsAuthResultSuccess(result))
    {
      var realmMessage = RealmHelpers.GetMessage(result);
      _logger.LogError("Error Message: {msg}", realmMessage);
      if (result == RealmAuthResult.WOW_FAIL_UNKNOWN_ACCOUNT)
      {
        // seems sometimes this error happens even on a legit connect. so just run regular reconnect loop
        EventCallback?.Invoke(new RealmErrorEvent()
        {
          Message = realmMessage
        });
      }
      else
      {
        EventCallback?.Invoke(new RealmErrorEvent()
        {
          Message = realmMessage,
        });
      }
      return;
    }

    var proof = msg.ByteBuf.ReadBytes(20).GetArrayCopy();
    if (SRPClient == null)
    {
      throw new InvalidOperationException("SRPClient was null when handling login proof");
    }

    if (!SRPClient.ExpectedProofResponse.SequenceEqual(proof))
    {
      _logger.LogError("Logon proof generated by client and server differ. Something is very wrong! Will try to reconnect in a moment.");
      // Also sometimes happens on a legit connect.
      EventCallback?.Invoke(new RealmErrorEvent()
      {
        Message = "Logon proof generated by client and server differ."
      });
      return;
    }

    var accountFlag = msg.ByteBuf.ReadIntLE(); // account flag

    EventCallback?.Invoke(new RealmAuthenticatedEvent()
    {
      SessionKey = SRPClient.SessionKey.ToCleanByteArray(),
      AccountFlag = accountFlag
    });
  }
}