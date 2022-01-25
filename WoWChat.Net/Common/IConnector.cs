namespace WoWChat.Net.Common
{
  using System.Threading.Tasks;

  public interface IConnector
  {
    /// <summary>
    /// Instructs the connector to connect.
    /// </summary>
    /// <returns></returns>
    Task Connect();
  }
}
