namespace WoWChat.Net.Common
{
  using DotNetty.Handlers.Timeout;
  using DotNetty.Transport.Channels;
  using Microsoft.Extensions.Logging;
  using System;

  public class IdleStateCallback : ChannelHandlerAdapter
  {
    private readonly ILogger<IdleStateCallback> _logger;

    public IdleStateCallback(ILogger<IdleStateCallback> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void UserEventTriggered(IChannelHandlerContext context, object evt)
    {
      if (evt is IdleStateEvent idleStateEvent)
      {
        var idler = idleStateEvent.State switch
        {
          IdleState.ReaderIdle => "reader",
          IdleState.WriterIdle => "writer",
          _ => "all",
        };
        _logger.LogError("Network state for {idler} marked as idle!", idler);
        context.CloseAsync().Wait();
      }

      base.UserEventTriggered(context, evt);
    }
  }
}