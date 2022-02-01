using WoWChat.Net;

public class Worker : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<Worker> _logger;

  public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
  {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var wowChat = scope.ServiceProvider.GetRequiredService<IWoWChat>();

    _logger.LogInformation("WoWChat started at: {time}", DateTimeOffset.Now);

    await wowChat.Run(stoppingToken);

    // Block this task until the program is closed.
    await Task.Delay(-1, stoppingToken);
  }
}