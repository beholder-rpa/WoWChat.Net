using WoWChat.Net.CLI;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
      var config = context.Configuration;

      services.AddWowChat(config);
      services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
