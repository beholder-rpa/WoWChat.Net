namespace WoWChat.Net;

using Common;
using Realm;

public delegate RealmPacketHandler? RealmPacketHandlerResolver(WoWExpansion expansion);
