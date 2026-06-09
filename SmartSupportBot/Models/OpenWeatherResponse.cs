using System.Text.Json.Serialization;

namespace SmartSupportBot.Models;

/// <summary>
/// Deserialization model for the OpenWeatherMap current-weather JSON payload.
/// Only the fields we need are mapped.
/// </summary>
internal class OpenWeatherResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sys")]
    public OpenWeatherSys? Sys { get; set; }

    [JsonPropertyName("main")]
    public OpenWeatherMain? Main { get; set; }

    [JsonPropertyName("weather")]
    public List<OpenWeatherCondition>? Weather { get; set; }

    [JsonPropertyName("wind")]
    public OpenWeatherWind? Wind { get; set; }
}

internal class OpenWeatherSys
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

internal class OpenWeatherMain
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

internal class OpenWeatherCondition
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

internal class OpenWeatherWind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}
