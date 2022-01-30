using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using WoWChat.Net;
using WoWChat.Net.CLI;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
      var envVarSource = app.Sources.OfType<EnvironmentVariablesConfigurationSource>().FirstOrDefault();
      if (envVarSource != null)
      {
        var envVarSourceIx = app.Sources.IndexOf(envVarSource);
        app.Sources.Insert(envVarSourceIx, new JsonConfigurationSource()
        {
          Path = "appsettings.local.json",
          Optional = true,
          ReloadOnChange = true,
        });
      }
      else
      {
        app.AddJsonFile("appsettings.local.json", true, true);
      }

    })
    .ConfigureServices((context, services) =>
    {
      var config = context.Configuration;

      services.AddWoWChat(config.GetSection("WoWChat"));
      services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
