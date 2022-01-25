namespace WoWChat.Net.CLI
{
  public class Worker : BackgroundService
  {
    private readonly IWoWChat _wowChat;
    private readonly ILogger<Worker> _logger;

    public Worker(IWoWChat wowChat, ILogger<Worker> logger)
    {
      _wowChat = wowChat ?? throw new ArgumentNullException(nameof(wowChat));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("WoWChat started at: {time}", DateTimeOffset.Now);
      await _wowChat.Run(stoppingToken);

      // Block this task until the program is closed.
      await Task.Delay(-1, stoppingToken);
    }
  }
}