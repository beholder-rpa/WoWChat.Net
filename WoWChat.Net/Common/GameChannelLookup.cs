namespace WoWChat.Net.Common;

using System.Collections.Concurrent;

public class GameChannelLookup
{
  private readonly ConcurrentDictionary<int, string> _channelTable = new();

  public bool TryGetChannel(int id, out string? channelName)
  {
    return _channelTable.TryGetValue(id, out channelName);
  }

  public bool TryGetChannel(string channelName, out int? id)
  {
    var channel = _channelTable.FirstOrDefault(channel => channel.Value == channelName);
    if (channel.Equals(default(KeyValuePair<int, string>)))
    {
      id = 0;
      return false;
    }
    id = channel.Key;
    return true;
  }

  public void AddOrUpdate(int id, string channelName)
  {
    _channelTable.AddOrUpdate(id, channelName, (id, existing) => channelName);
  }

  public void Remove(int id, out string? channelName)
  {
    _channelTable.Remove(id, out channelName);
  }

  public void Clear()
  {
    _channelTable.Clear();
  }
}