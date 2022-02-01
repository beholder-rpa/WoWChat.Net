﻿namespace WoWChat.Net.Tests;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Extensions;
using Game;
using Game.PacketHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Options;
using System;
using System.Threading.Tasks;
using Xunit;

public class ServerAuthChallengePacketHandlerTests
{
  [Fact]
  public void Ensure_ServerAuthChallengePacketHandlerWotLK_Creates_Valid_Packets()
  {
    // Arrange
    var wowChatOptionsSnapshot = new Mock<IOptionsSnapshot<WowChatOptions>>();
    wowChatOptionsSnapshot.Setup(m => m.Value).Returns(new WowChatOptions()
    {
      WoW = new WoWOptions()
      {
        AccountName = "overhill"
      }
    });
    var loggerMock = new Mock<ILogger<ServerAuthChallengePacketHandlerWotLK>>();

    static GameHeaderCrypt headerCryptResolver(WoWExpansion exp) => new GameHeaderCryptWotLK();

    var packetHandler = new ServerAuthChallengePacketHandlerWotLK(wowChatOptionsSnapshot.Object, headerCryptResolver, loggerMock.Object);
    var contextMock = new Mock<IChannelHandlerContext>(MockBehavior.Strict);
    contextMock.Setup(x => x.Allocator).Returns(UnpooledByteBufferAllocator.Default);
    Packet? observedPacket = null;
    contextMock.Setup(x => x.WriteAndFlushAsync(It.IsAny<Packet>()))
      .Callback((object message) => observedPacket = Assert.IsAssignableFrom<Packet>(message))
      .Returns(Task.CompletedTask);

    var allocator = UnpooledByteBufferAllocator.Default;
    var byteBuff = allocator.Buffer();
    byteBuff.WriteBytes(new byte[]
    {
     0x01, 0x00, 0x00, 0x00, 0x20, 0x84, 0x64, 0x3E, 0x3B, 0x90, 0x76, 0x11, 0x3F, 0xE0, 0xB3, 0x30, 0xCE, 0xB4, 0x09, 0xE5, 0xBC, 0xAD, 0x6F, 0xE9, 0xDB, 0x7C, 0xD7, 0x40, 0x35, 0x63, 0xD9, 0xE7, 0x24, 0x04, 0x5B, 0x22, 0xC5, 0x38, 0x0E, 0xCB,
    });

    packetHandler.Realm = new GameServerInfo()
    {
      RealmId = 11,
    };

    packetHandler.SessionKey = new byte[]
    {
      0x77, 0x96, 0x8E, 0xB0, 0x54, 0xD1, 0xC2, 0xA2, 0x93, 0x5D, 0x8D, 0xB4, 0x0B, 0xD2, 0x53, 0x76, 0x74, 0x18, 0xB2, 0x24, 0xE4, 0x30, 0x24, 0x7B, 0x1A, 0x3B, 0x59, 0x47, 0x21, 0xAE, 0xD5, 0x69, 0x5A, 0xB9, 0xAF, 0xD4, 0x5B, 0x9C, 0xBE, 0x6C,
    };

    packetHandler.ClientSeed = -1578654646;
    // Act
    packetHandler.HandlePacket(contextMock.Object, new Packet(WorldCommand.SMSG_AUTH_CHALLENGE, byteBuff));

    // Assert
    contextMock.Verify(x => x.WriteAndFlushAsync(It.IsAny<Packet>()), Times.Once);
    Assert.NotNull(observedPacket);
    Assert.NotNull(packetHandler.ServerSeed);
    Assert.Equal(WorldCommand.CMSG_AUTH_CHALLENGE, observedPacket?.Id);

    var actualPacketContents = observedPacket?.ByteBuf.GetArrayCopy();

    var expectedPacketContents = new byte[] {
      0x00, 0x00, 0x34, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4F, 0x56, 0x45, 0x52, 0x48, 0x49, 0x4C, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0xE7, 0xA4, 0x4A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x97, 0x0D, 0x01, 0xA3, 0x66, 0x56, 0x44, 0x9A, 0x10, 0xF3, 0x3D, 0x60, 0x50, 0x88, 0x81, 0x5C, 0xD6, 0x24, 0x46, 0x9E, 0x02, 0x00, 0x00, 0x78, 0x9C, 0x75, 0xD2, 0xC1, 0x6A, 0xC3, 0x30, 0x0C, 0xC6, 0x71, 0xEF, 0x29, 0x76, 0xE9, 0x9B, 0xEC, 0xB4, 0xB4, 0x50, 0xC2, 0xEA, 0xCB, 0xE2, 0x9E, 0x8B, 0x62, 0x7F, 0x4B, 0x44, 0x6C, 0x39, 0x38, 0x4E, 0xB7, 0xF6, 0x3D, 0xFA, 0xBE, 0x65, 0xB7, 0x0D, 0x94, 0xF3, 0x4F, 0x48, 0xF0, 0x47, 0xAF, 0xC6, 0x98, 0x26, 0xF2, 0xFD, 0x4E, 0x25, 0x5C, 0xDE, 0xFD, 0xC8, 0xB8, 0x22, 0x41, 0xEA, 0xB9, 0x35, 0x2F, 0xE9, 0x7B, 0x77, 0x32, 0xFF, 0xBC, 0x40, 0x48, 0x97, 0xD5, 0x57, 0xCE, 0xA2, 0x5A, 0x43, 0xA5, 0x47, 0x59, 0xC6, 0x3C, 0x6F, 0x70, 0xAD, 0x11, 0x5F, 0x8C, 0x18, 0x2C, 0x0B, 0x27, 0x9A, 0xB5, 0x21, 0x96, 0xC0, 0x32, 0xA8, 0x0B, 0xF6, 0x14, 0x21, 0x81, 0x8A, 0x46, 0x39, 0xF5, 0x54, 0x4F, 0x79, 0xD8, 0x34, 0x87, 0x9F, 0xAA, 0xE0, 0x01, 0xFD, 0x3A, 0xB8, 0x9C, 0xE3, 0xA2, 0xE0, 0xD1, 0xEE, 0x47, 0xD2, 0x0B, 0x1D, 0x6D, 0xB7, 0x96, 0x2B, 0x6E, 0x3A, 0xC6, 0xDB, 0x3C, 0xEA, 0xB2, 0x72, 0x0C, 0x0D, 0xC9, 0xA4, 0x6A, 0x2B, 0xCB, 0x0C, 0xAF, 0x1F, 0x6C, 0x2B, 0x52, 0x97, 0xFD, 0x84, 0xBA, 0x95, 0xC7, 0x92, 0x2F, 0x59, 0x95, 0x4F, 0xE2, 0xA0, 0x82, 0xFB, 0x2D, 0xAA, 0xDF, 0x73, 0x9C, 0x60, 0x49, 0x68, 0x80, 0xD6, 0xDB, 0xE5, 0x09, 0xFA, 0x13, 0xB8, 0x42, 0x01, 0xDD, 0xC4, 0x31, 0x6E, 0x31, 0x0B, 0xCA, 0x5F, 0x7B, 0x7B, 0x1C, 0x3E, 0x9E, 0xE1, 0x93, 0xC8, 0x8D,
    };

    var expected = $"Expected: {BitConverter.ToString(expectedPacketContents)}";
    var actual = $"  Actual: {BitConverter.ToString(actualPacketContents ?? Array.Empty<byte>())}";

    Assert.Equal(expectedPacketContents, actualPacketContents);
  }
}
