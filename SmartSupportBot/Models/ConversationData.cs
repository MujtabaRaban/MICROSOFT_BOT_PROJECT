namespace SmartSupportBot.Models;

/// <summary>Per-conversation state persisted via <see cref="Microsoft.Bot.Builder.ConversationState"/>.</summary>
public class ConversationData
{
    /// <summary>User display name collected during onboarding (also stored in UserState).</summary>
    public string? UserName { get; set; }

    /// <summary>Total messages in this conversation.</summary>
    public int MessageCount { get; set; }

    /// <summary>UTC timestamp of the last user message.</summary>
    public DateTime LastInteractionUtc { get; set; } = DateTime.UtcNow;
}
