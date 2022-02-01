namespace WoWChat.Net;

using Common;
using System.Threading;

public interface IWoWChat : IObservable<IWoWChatEvent>
{
  Task Run(CancellationToken cancellationToken);
}
