namespace WoWChat.Net
{
  using Realm;
  using System;

  public class WoWChat : IWoWChat
  {
    private readonly RealmConnector _realmConnector;

    public WoWChat(RealmConnector realmConnector)
    {
      _realmConnector = realmConnector ?? throw new ArgumentNullException(nameof(realmConnector));
    }

    public async Task Run(CancellationToken cancellationToken)
    {
      await _realmConnector.Connect();
    }
  }
}
