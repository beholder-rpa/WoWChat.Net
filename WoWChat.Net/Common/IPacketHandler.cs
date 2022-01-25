namespace WoWChat.Net.Common
{
  public interface IPacketHandler
  {
    void ChannelActive(IConnector connector);
  }
}
