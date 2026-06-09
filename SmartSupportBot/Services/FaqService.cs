namespace SmartSupportBot.Services;

/// <summary>
/// FAQ module backed by a hardcoded <see cref="Dictionary{TKey,TValue}"/>.
/// Keeps answers separate from bot orchestration logic.
/// </summary>
public class FaqService
{
    private readonly ILogger<FaqService> _logger;

    /// <summary>Keyword → answer map (case-insensitive).</summary>
    private static readonly Dictionary<string, string> FaqEntries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["help"] = GetHelpText(),
        ["faq"] = GetFaqListText(),
        ["hours"] = "Our support team is available Monday–Friday, 9 AM – 6 PM (local time).",
        ["contact"] = "You can reach us at support@smartsupport.example or call 1-800-SUPPORT.",
        ["pricing"] = "We offer Free, Pro ($9/mo), and Enterprise (custom) plans. Visit smartsupport.example/pricing.",
        ["refund"] = "Refunds are available within 30 days of purchase. Contact support with your order ID.",
        ["shipping"] = "Standard shipping takes 3–5 business days. Express shipping is available at checkout."
    };

    public FaqService(ILogger<FaqService> logger)
    {
        _logger = logger;
    }

    /// <summary>Looks up an FAQ answer by exact keyword or substring match.</summary>
    public bool TryGetAnswer(string message, out string answer)
    {
        answer = string.Empty;
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        var normalized = message.Trim();

        if (FaqEntries.TryGetValue(normalized, out var exact))
        {
            _logger.LogDebug("FAQ exact match for: {Keyword}", normalized);
            answer = exact;
            return true;
        }

        foreach (var (keyword, response) in FaqEntries)
        {
            if (keyword is "help" or "faq")
            {
                continue;
            }

            if (normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("FAQ keyword match: {Keyword}", keyword);
                answer = response;
                return true;
            }
        }

        return false;
    }

    public static string GetHelpText() =>
        "Here's what I can do:\n" +
        "• **help** — show this message\n" +
        "• **faq** — list FAQ topics\n" +
        "• **hours**, **contact**, **pricing**, **refund**, **shipping** — quick answers\n" +
        "• **weather &lt;city&gt;** — current weather (e.g. `weather London`)\n" +
        "• Ask me anything else and I'll do my best!";

    public static string GetFaqListText() =>
        "FAQ topics: **hours**, **contact**, **pricing**, **refund**, **shipping**\n" +
        "Type a topic name or ask a question containing one of these keywords.";
}
