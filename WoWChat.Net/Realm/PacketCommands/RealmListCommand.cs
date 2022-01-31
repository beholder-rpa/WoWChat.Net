namespace WoWChat.Net.Realm.PacketCommands;

using Common;
using DotNetty.Buffers;
using Events;

[PacketCommand(RealmCommand.CMD_REALM_LIST, WoWExpansion.All)]
public class RealmListCommand : IPacketCommand<RealmEvent>
{
  public int CommandId { get; set; }

  public Action<RealmEvent>? EventCallback { get; set; }

  public Task<Packet> CreateCommandPacket(IByteBufferAllocator allocator)
  {
    // ask for realm list
    var ret = allocator.Buffer(4, 4);
    ret.WriteIntLE(0);
    return Task.FromResult(new Packet(RealmCommand.CMD_REALM_LIST, ret));
  }
}
