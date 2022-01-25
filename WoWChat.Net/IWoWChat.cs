namespace WoWChat.Net
{
  using System.Threading;

  public interface IWoWChat
  {
    Task Run(CancellationToken cancellationToken);
  }
}