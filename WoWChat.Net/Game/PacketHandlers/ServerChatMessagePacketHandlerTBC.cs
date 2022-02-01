namespace WoWChat.Net.Game.PacketHandlers;

using Common;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System.Text;

[PacketHandler(WorldCommand.SMSG_MESSAGECHAT, WoWExpansion.TBC)]
public class ServerChatMessagePacketHandlerTBC : ServerChatMessagePacketHandler
{
  public ServerChatMessagePacketHandlerTBC(GameNameLookup nameLookup, IOptionsSnapshot<WowChatOptions> options, ILogger<ServerChatMessagePacketHandlerTBC> logger)
    : base(nameLookup, options, logger)
  {
  }

  protected override GameChatMessage ParseChatMessage(Packet msg)
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
    var text = string.Empty;
    if (textLength > 0)
    {
      text = msg.ByteBuf.ReadCharSequence(textLength - 1, Encoding.UTF8).ToString();
    }

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

  private class ChatMessageType
  {
    public const byte CHAT_MSG_SYSTEM = 0x00;
    public const byte CHAT_MSG_SAY = 0x01;
    public const byte CHAT_MSG_GUILD = 0x04;
    public const byte CHAT_MSG_OFFICER = 0x05;
    public const byte CHAT_MSG_YELL = 0x06;
    public const byte CHAT_MSG_WHISPER = 0x07;
    public const byte CHAT_MSG_EMOTE = 0x0A;
    public const byte CHAT_MSG_TEXT_EMOTE = 0x0B;
    public const byte CHAT_MSG_CHANNEL = 0x11;

    public const byte CHAT_MSG_CHANNEL_JOIN = 0x12;
    public const byte CHAT_MSG_CHANNEL_LEAVE = 0x13;
    public const byte CHAT_MSG_CHANNEL_LIST = 0x14;
    public const byte CHAT_MSG_CHANNEL_NOTICE = 0x15;
    public const byte CHAT_MSG_CHANNEL_NOTICE_USER = 0x16;

    public const byte CHAT_MSG_ACHIEVEMENT = 0x30;
    public const byte CHAT_MSG_GUILD_ACHIEVEMENT = 0x31;
  }
}