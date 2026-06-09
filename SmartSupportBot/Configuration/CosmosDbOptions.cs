namespace SmartSupportBot.Configuration;

/// <summary>
/// Cosmos DB settings for durable bot state. When not configured, the app falls back to in-memory storage.
/// </summary>
public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    public string? Endpoint { get; set; }
    public string? AuthKey { get; set; }
    public string DatabaseId { get; set; } = "SmartSupportBot";
    public string ContainerId { get; set; } = "BotState";

    /// <summary>True when endpoint and key are set to real (non-placeholder) values.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(AuthKey) &&
        !Endpoint.Contains("YOUR_COSMOS", StringComparison.OrdinalIgnoreCase) &&
        !AuthKey.Contains("YOUR_COSMOS", StringComparison.OrdinalIgnoreCase);
}
