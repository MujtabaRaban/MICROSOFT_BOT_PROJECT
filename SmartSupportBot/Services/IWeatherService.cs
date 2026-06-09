namespace SmartSupportBot.Services;

/// <summary>Weather lookup abstraction for testability and clean DI.</summary>
public interface IWeatherService
{
    /// <summary>Returns a formatted weather summary, or null if the city was not found.</summary>
    Task<string?> GetWeatherTextAsync(string city, CancellationToken cancellationToken = default);
}
