namespace WoWChat.Net.Game
{
  using Common;
  using DotNetty.Buffers;
  using DotNetty.Codecs;
  using DotNetty.Transport.Channels;
  using Extensions;
  using Helpers;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Options;
  using System;
  using System.Collections.Generic;

  public class GamePacketDecoderWotLK : GamePacketDecoder
  {
    public GamePacketDecoderWotLK(IOptionsSnapshot<WowChatOptions> options, ILogger<GamePacketDecoder> logger)
      : base(options, logger)
    {
    }
  }
}
