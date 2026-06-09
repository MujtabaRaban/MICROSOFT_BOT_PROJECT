namespace SmartSupportBot.Models;

/// <summary>Configuration section for the OpenWeatherMap API.</summary>
public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5/weather";
}
