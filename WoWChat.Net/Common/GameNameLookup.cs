namespace WoWChat.Net.Common
{
  using System.Collections.Concurrent;

  public class GameNameLookup
  {
    private readonly ConcurrentDictionary<long, GameNameQuery> _nameTable = new();

    public bool TryGetName(long guid, out GameNameQuery? gameNameQuery)
    {
      return _nameTable.TryGetValue(guid, out gameNameQuery);
    }

    public void AddOrUpdate(GameNameQuery gameName)
    {
      _nameTable.AddOrUpdate(gameName.Id, gameName, (id, existing) => gameName);
    }

    public void Remove(long guid, out GameNameQuery? gameName)
    {
      _nameTable.Remove(guid, out gameName);
    }

    public void Clear()
    {
      _nameTable.Clear();
    }
  }
}