namespace SmartSupportBot.Models;

/// <summary>
/// Persisted per-user data. Survives across conversations for the same user.
/// </summary>
public class UserProfile
{
    /// <summary>Display name collected during the onboarding dialog.</summary>
    public string? Name { get; set; }

    /// <summary>True once the user has completed the name-collection waterfall.</summary>
    public bool OnboardingComplete { get; set; }
}
