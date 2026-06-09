using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartSupportBot.Models;

namespace SmartSupportBot.Services;

/// <summary>
/// Fetches weather from OpenWeatherMap using an <see cref="HttpClient"/> injected via DI.
/// Returns a clean, human-readable text summary (not raw JSON).
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> GetWeatherTextAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey) ||
            _options.ApiKey.Equals("YOUR_OPENWEATHERMAP_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Weather API key is not configured.");
            throw new InvalidOperationException(
                "Weather API key is not configured. Set WeatherApi:ApiKey in appsettings.json.");
        }

        var encodedCity = Uri.EscapeDataString(city.Trim());
        var url = $"{_options.BaseUrl}?q={encodedCity}&appid={_options.ApiKey}&units=metric";

        _logger.LogInformation("Fetching weather for: {City}", city);

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("City not found: {City}", city);
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<OpenWeatherResponse>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (payload?.Main is null)
        {
            return null;
        }

        var description = payload.Weather?.FirstOrDefault()?.Description ?? "N/A";
        var country = payload.Sys?.Country ?? string.Empty;

        return
            $"**Weather in {payload.Name}, {country}**\n" +
            $"• Conditions: {description}\n" +
            $"• Temperature: {payload.Main.Temp:F1} °C (feels like {payload.Main.FeelsLike:F1} °C)\n" +
            $"• Humidity: {payload.Main.Humidity}%\n" +
            $"• Wind: {payload.Wind?.Speed ?? 0:F1} m/s";
    }
}
