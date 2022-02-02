namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using DotNetty.Transport.Channels;
using Events;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Text;
using System.Text.RegularExpressions;

[PacketHandler(WorldCommand.SMSG_MESSAGECHAT, WoWExpansion.Vanilla)]
public class ServerChatMessagePacketHandler : IPacketHandler<GameEvent>
{
  protected readonly GameNameLookup _gameNameLookup;
  protected readonly WowChatOptions _options;
  protected readonly ILogger<ServerChatMessagePacketHandler> _logger;

  public ServerChatMessagePacketHandler(GameNameLookup gameNameLookup, IOptionsSnapshot<WowChatOptions> options, ILogger<ServerChatMessagePacketHandler> logger)
  {
    _gameNameLookup = gameNameLookup ?? throw new ArgumentNullException(nameof(gameNameLookup));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public Action<GameEvent>? EventCallback { get; set; }

  public void HandlePacket(IChannelHandlerContext ctx, Packet msg)
  {
    if (_logger.IsEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("RECV GAME CHAT: {id} - {byteBuf}", BitConverter.ToString(msg.Id.ToBytes()), BitConverter.ToString(msg.ByteBuf.GetArrayCopy()));
    }

    var chatMessage = ParseChatMessage(msg);

    var shouldRaiseEvent = true;
    if (_options.WoW.Filters.Enabled)
    {
      var text = chatMessage.FormattedMessage;
      foreach (var pattern in _options.WoW.Filters.Patterns)
      {
        if (Regex.IsMatch(text, pattern))
        {
          shouldRaiseEvent = false;
          continue;
        }
      }
    }

    if (shouldRaiseEvent)
    {
      EventCallback?.Invoke(new GameChatMessageEvent()
      {
        ChatMessage = chatMessage
      });
    }
    else
    {
      _logger.LogInformation("Message was filtered: {filteredMessage}", chatMessage.FormattedMessage);
    }
  }

  protected virtual GameChatMessage ParseChatMessage(Packet msg)
  {
    var messageType = msg.ByteBuf.ReadByte();
    var messageLanguage = msg.ByteBuf.ReadIntLE();

    string channelName = string.Empty;
    if (messageType == ChatMessageType.CHAT_MSG_CHANNEL)
    {
      channelName = msg.ByteBuf.ReadString();
    }

    var senderId = msg.ByteBuf.ReadLongLE();

    long targetId = default;
    if (messageType == ChatMessageType.CHAT_MSG_SAY ||
        messageType == ChatMessageType.CHAT_MSG_YELL)
    {
      targetId = msg.ByteBuf.ReadLongLE();
    }

    var textLength = msg.ByteBuf.ReadIntLE();
    var text = msg.ByteBuf.ReadCharSequence(textLength - 1, Encoding.UTF8).ToString();

    return new GameChatMessage()
    {
      MessageType = (Common.ChatMessageType)messageType,
      Language = (Language)messageLanguage,
      ChannelName = channelName,
      SenderId = senderId,
      TargetId = targetId,
      Message = text
    };
  }

  protected virtual string GetFormattedMessage(
    Common.ChatMessageType messageType,
    long senderId,
    Language language,
    string addonName,
    string channelName,
    string message
    )
  {
    var senderName = senderId.ToString();
    if (_gameNameLookup.TryGetName(senderId, out var gameName))
    {
      senderName = gameName?.Name;
    }
    else if (senderId != 0)
    {
      EventCallback?.Invoke(new GameNameQueryRequestEvent()
      {
        Guid = senderId,
      });
    }

    //TODO: use user-provided formatting supplied in options.
    //supported tokens: %time, %type, %language, %user, %message, and %channel
    string? formattedMessage;

    string? header;
    if (senderId == 0)
    {
      header = $"[{messageType}]:";
    }
    else
    {
      header = $"[{messageType}:{senderName}]:";
    }

    if (language == Language.Addon)
    {
      formattedMessage = $"{addonName} {header} {message}";
    }
    else if (string.IsNullOrWhiteSpace(channelName))
    {
      formattedMessage = $"{header} ({language}) {message}";
    }
    else
    {
      formattedMessage = $"{header} {channelName} ({language}) {message}";
    }

    return formattedMessage;
  }

  private class ChatMessageType
  {
    public const byte CHAT_MSG_SAY = 0x00;
    public const byte CHAT_MSG_GUILD = 0x03;
    public const byte CHAT_MSG_OFFICER = 0x04;
    public const byte CHAT_MSG_YELL = 0x05;
    public const byte CHAT_MSG_WHISPER = 0x06;
    public const byte CHAT_MSG_EMOTE = 0x08;
    public const byte CHAT_MSG_TEXT_EMOTE = 0x09;
    public const byte CHAT_MSG_CHANNEL = 0x0E;
    public const byte CHAT_MSG_SYSTEM = 0x0A;
    public const byte CHAT_MSG_CHANNEL_JOIN = 0x0F;
    public const byte CHAT_MSG_CHANNEL_LEAVE = 0x10;
    public const byte CHAT_MSG_CHANNEL_LIST = 0x11;
    public const byte CHAT_MSG_CHANNEL_NOTICE = 0x12;
    public const byte CHAT_MSG_CHANNEL_NOTICE_USER = 0x13;

    public const byte CHAT_MSG_ACHIEVEMENT = 0x2E;
    public const byte CHAT_MSG_GUILD_ACHIEVEMENT = 0x2F;
  }
}