namespace WoWChat.Net.Game.Events;

using Common;

public record GameWeatherEvent : GameEvent
{
  public WeatherState State { get; init; }

  public double Intensity { get; init; }

  public bool IsAbrupt { get; init; }
}