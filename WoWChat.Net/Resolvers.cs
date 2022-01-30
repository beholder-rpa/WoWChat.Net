namespace WoWChat.Net;

using Common;
using Game;
using Realm;

public delegate RealmPacketHandler RealmPacketHandlerResolver(WoWExpansion expansion);

public delegate GamePacketHandler GamePacketHandlerResolver(WoWExpansion expansion);

public delegate GamePacketDecoder GamePacketDecoderResolver(WoWExpansion expansion);

public delegate GamePacketEncoder GamePacketEncoderResolver(WoWExpansion expansion);

public delegate GameHeaderCrypt GameHeaderCryptResolver(WoWExpansion expansion);
