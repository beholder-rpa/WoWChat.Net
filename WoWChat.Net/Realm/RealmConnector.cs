namespace WoWChat.Net.Realm;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using System;
using System.Threading.Tasks;

public class RealmConnector : IConnector
{
  private readonly RealmChannelHandler _realmChannelHandler;
  private readonly IEventLoopGroup _group;
  private readonly WowChatOptions _options;
  private readonly ILogger<RealmConnector> _logger;

  private IChannel? _realmChannel;

  public RealmConnector(RealmChannelHandler realmChannelHandler, IEventLoopGroup group, IOptions<WowChatOptions> options, ILogger<RealmConnector> logger)
  {
    _realmChannelHandler = realmChannelHandler ?? throw new ArgumentNullException(nameof(realmChannelHandler));
    _group = group ?? throw new ArgumentNullException(nameof(group));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task Connect()
  {
    if (_realmChannel != null && _realmChannel.Active)
    {
      throw new InvalidOperationException($"Refusing to connect to realm server. Already connected.");
    }

    _logger.LogInformation("Connecting to game server {realmName} ({host}:{port})", _options.RealmName, _options.RealmListHost, _options.RealmListPort);

    var bootstrap = new Bootstrap();
    bootstrap.Group(_group)
      .Channel<TcpSocketChannel>()
      .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(_options.ConnectTimeoutMs))
      .Option(ChannelOption.SoKeepalive, true)
      .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
      .RemoteAddress(_options.RealmListHost, _options.RealmListPort)
      .Handler(_realmChannelHandler);

    try
    {
      var cancelTask = Task.Delay(_options.ConnectTimeoutMs);
      var connectTask = bootstrap.ConnectAsync();

      //double await so if cancelTask throws exception, this throws it
      await await Task.WhenAny(connectTask, cancelTask);

      if (cancelTask.IsCompleted)
      {
        //If cancelTask and connectTask both finish at the same time,
        //we'll consider it to be a timeout. 
        throw new TimeoutException();
      }

      _realmChannel = connectTask.Result;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to connect to realm server! {message}", ex.Message);
      //realmConnectionCallback.OnDisconnected();
    }
  }
}
