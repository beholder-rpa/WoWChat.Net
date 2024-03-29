﻿namespace WoWChat.Net.Tests;

using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Options;
using Realm;
using Realm.PacketHandlers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class LogonAuthChallengePacketHandlerTests
{
  [Fact]
  public void Ensure_ServerAuthChallengePacketHandlerWotLK_Creates_Valid_Proof()
  {
    // Arrange
    var wowChatOptionsSnapshot = new Mock<IOptionsSnapshot<WowChatOptions>>();
    wowChatOptionsSnapshot.Setup(m => m.Value).Returns(new WowChatOptions()
    {
      WoW = new WoWOptions()
      {
        AccountName = "overhill",
        AccountPassword = "SomePassword",
        Version = "3.3.5",
        Platform = Platform.Windows,
      }
    });
    var loggerMock = new Mock<ILogger<LogonAuthChallengePacketHandler>>();

    var packetHandler = new LogonAuthChallengePacketHandler(wowChatOptionsSnapshot.Object, loggerMock.Object);
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
     0x00, 0x00, 0x75, 0xA1, 0xB0, 0xEC, 0x9A, 0x8F, 0x65, 0xCB, 0x45, 0xDF, 0x02, 0x37, 0xD5, 0x75, 0xFB, 0x60, 0xE3, 0x4A, 0xA4, 0x01, 0x12, 0x3B, 0xBA, 0xC6, 0xD2, 0x44, 0x88, 0x7F, 0x8B, 0x44, 0x19, 0x09, 0x01, 0x07, 0x20, 0xB7, 0x9B, 0x3E, 0x2A, 0x87, 0x82, 0x3C, 0xAB, 0x8F, 0x5E, 0xBF, 0xBF, 0x8E, 0xB1, 0x01, 0x08, 0x53, 0x50, 0x06, 0x29, 0x8B, 0x5B, 0xAD, 0xBD, 0x5B, 0x53, 0xE1, 0x89, 0x5E, 0x64, 0x4B, 0x89, 0xF9, 0x82, 0x5D, 0x7B, 0xC8, 0xAA, 0x0F, 0x9F, 0x5D, 0x95, 0x5A, 0xC1, 0xAC, 0x2D, 0xB0, 0xCA, 0xBD, 0xCF, 0x23, 0xFB, 0x4C, 0x4D, 0x9B, 0x75, 0x4F, 0xED, 0xD8, 0x82, 0xA6, 0x94, 0x79, 0xC9, 0xBA, 0xA3, 0x1E, 0x99, 0xA0, 0x0B, 0x21, 0x57, 0xFC, 0x37, 0x3F, 0xB3, 0x69, 0xCD, 0xD2, 0xF1, 0x00,
    });

    packetHandler.InitialKeyPair = new byte[]
    {
     0xBF, 0x52, 0xD5, 0x7C, 0xE9, 0xF7, 0x16, 0x3B, 0xCD, 0xF8, 0x21, 0x86, 0x96, 0x6D, 0x76, 0xE6, 0x28, 0xCD, 0x79,
    };

    // Act
    packetHandler.HandlePacket(contextMock.Object, new Packet(RealmCommand.CMD_AUTH_LOGON_CHALLENGE, byteBuff));

    // Assert
    contextMock.Verify(x => x.WriteAndFlushAsync(It.IsAny<Packet>()), Times.Once);
    Assert.NotNull(packetHandler.SRPClient);

    Assert.Equal(RealmCommand.CMD_AUTH_LOGON_PROOF, observedPacket?.Id);

    var actualPacketContents = observedPacket?.ByteBuf.GetArrayCopy();

    var expectedSPRClientA = new byte[]
    {
      0x73, 0x78, 0x95, 0x4B, 0x5C, 0xE6, 0xD1, 0x74, 0x99, 0x91, 0x1C, 0xFE, 0x8B, 0x51, 0x41, 0x31, 0xC6, 0x22, 0xFF, 0xEC, 0x5C, 0xE6, 0x7D, 0x5B, 0xC4, 0x74, 0x9C, 0x52, 0xFE, 0x71, 0x27, 0x45,
    };

    var expectedSessionKey = new byte[]
    {
      0xDC, 0x33, 0xC9, 0x17, 0xF0, 0x92, 0x8E, 0x77, 0x2E, 0x2B, 0x6B, 0x3E, 0xC1, 0x79, 0x12, 0xF4, 0x43, 0x51, 0x08, 0xDE, 0x3C, 0xEB, 0xC1, 0x0A, 0x8C, 0x7A, 0xBA, 0xFE, 0x15, 0xE2, 0xE0, 0x46, 0x42, 0xD7, 0xAF, 0xFF, 0xCE, 0x89, 0x6D, 0x58,
    };

    var expectedProof = new byte[]
    {
      0x46, 0xE0, 0xFA, 0x39, 0xBD, 0xAA, 0xCA, 0xEB, 0x4A, 0x52, 0x4A, 0x51, 0xF5, 0x5B, 0xE5, 0x7B, 0x7C, 0x5F, 0x3F, 0xCF,
    };

    var expectedPacketContents = new byte[] {
      0x73, 0x78, 0x95, 0x4B, 0x5C, 0xE6, 0xD1, 0x74, 0x99, 0x91, 0x1C, 0xFE, 0x8B, 0x51, 0x41, 0x31, 0xC6, 0x22, 0xFF, 0xEC, 0x5C, 0xE6, 0x7D, 0x5B, 0xC4, 0x74, 0x9C, 0x52, 0xFE, 0x71, 0x27, 0x45, 0xCF, 0x3F, 0x5F, 0x7C, 0x7B, 0xE5, 0x5B, 0xF5, 0x51, 0x4A, 0x52, 0x4A, 0xEB, 0xCA, 0xAA, 0xBD, 0x39, 0xFA, 0xE0, 0x46, 0xF9, 0x62, 0x98, 0x51, 0x96, 0x60, 0xD7, 0x67, 0xFA, 0x6A, 0xCD, 0xD5, 0x08, 0x36, 0xC7, 0x87, 0x78, 0xD4, 0xF3, 0x5E, 0x00, 0x00,
    };

    var expectedA = $"Expected A: {BitConverter.ToString(expectedSPRClientA)}";
    var actualA = $"  Actual A: {BitConverter.ToString(packetHandler.SRPClient?.A.ToByteArray() ?? Array.Empty<byte>())}";

    var expectedSessionKeyStr = $"Expected Session Key: {BitConverter.ToString(expectedSessionKey)}";
    var actualSessionKeyStr = $"  Actual Session Key: {BitConverter.ToString(packetHandler.SRPClient?.SessionKey.ToCleanByteArray() ?? Array.Empty<byte>())}";

    var expectedProofStr = $"Expected Proof: {BitConverter.ToString(expectedProof)}";
    var actualProofStr = $"  Actual Proof: {BitConverter.ToString(packetHandler.SRPClient?.Proof.Reverse().ToArray() ?? Array.Empty<byte>())}";

    var expected = $"Expected Packet: {BitConverter.ToString(expectedPacketContents)}";
    var actual = $"  Actual Packet: {BitConverter.ToString(actualPacketContents ?? Array.Empty<byte>())}";

    Assert.Equal(expectedSPRClientA, packetHandler.SRPClient?.A.ToByteArray());
    Assert.Equal(expectedSessionKey, packetHandler.SRPClient?.SessionKey.ToCleanByteArray());
    Assert.Equal(expectedProof, packetHandler.SRPClient?.Proof.Reverse().ToArray());
    Assert.Equal(expectedPacketContents, actualPacketContents);
  }
}