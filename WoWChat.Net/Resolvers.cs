namespace WoWChat.Net;

using Common;
using Game;

public delegate GamePacketDecoder GamePacketDecoderResolver(WoWExpansion expansion);

public delegate GamePacketEncoder GamePacketEncoderResolver(WoWExpansion expansion);

public delegate GameHeaderCrypt GameHeaderCryptResolver(WoWExpansion expansion);
