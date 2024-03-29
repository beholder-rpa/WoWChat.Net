﻿namespace WoWChat.Net.Game.Events;

using Common;

public record GameConnectedEvent : GameEvent
{
  public GameServerInfo GameServerInfo { get; init; } = new GameServerInfo();
}