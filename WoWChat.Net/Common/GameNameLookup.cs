namespace WoWChat.Net.Common
{
  using System.Collections.Concurrent;

  public class GameNameLookup
  {
    private ConcurrentDictionary<long, GameNameQuery> _nameDictionary = new ConcurrentDictionary<long, GameNameQuery>();

    public bool TryGetName(long guid, out GameNameQuery? gameNameQuery)
    {
      return _nameDictionary.TryGetValue(guid, out gameNameQuery);
    }

    public void AddOrUpdate(GameNameQuery gameName)
    {
      _nameDictionary.AddOrUpdate(gameName.Id, gameName, (id, existing) => gameName);
    }

    public void Remove(long guid, out GameNameQuery? gameName)
    {
      _nameDictionary.Remove(guid, out gameName);
    }
  }
}