using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using SmartSupportBot.Dialogs;
using SmartSupportBot.Models;
using SmartSupportBot.Services;

namespace SmartSupportBot.Bots;

/// <summary>
/// Main bot — extends <see cref="ActivityHandler"/>.
/// Routes messages to the onboarding dialog, FAQ, weather, or fallback handler.
/// </summary>
public class SupportBot : ActivityHandler
{
    private readonly ConversationState _conversationState;
    private readonly UserState _userState;
    private readonly MainDialog _mainDialog;
    private readonly IWeatherService _weatherService;
    private readonly FaqService _faqService;
    private readonly ILogger<SupportBot> _logger;

    private readonly IStatePropertyAccessor<ConversationData> _conversationAccessor;
    private readonly IStatePropertyAccessor<UserProfile> _userAccessor;
    private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;

    public SupportBot(
        ConversationState conversationState,
        UserState userState,
        MainDialog mainDialog,
        IWeatherService weatherService,
        FaqService faqService,
        ILogger<SupportBot> logger)
    {
        _conversationState = conversationState;
        _userState = userState;
        _mainDialog = mainDialog;
        _weatherService = weatherService;
        _faqService = faqService;
        _logger = logger;

        _conversationAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        _userAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
        _dialogStateAccessor = conversationState.CreateProperty<DialogState>(nameof(DialogState));
    }

    /// <summary>Greeting when a new user joins the conversation.</summary>
    protected override async Task OnMembersAddedAsync(
        IList<ChannelAccount> membersAdded,
        ITurnContext<IConversationUpdateActivity> turnContext,
        CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                _logger.LogInformation("New member joined: {MemberId}", member.Id);

                await turnContext.SendActivityAsync(
                    MessageFactory.Text(
                        "Hello! I'm **Smart Support Bot**. Send me a message to get started."),
                    cancellationToken);
            }
        }
    }

    /// <summary>Handles every incoming text message from the user.</summary>
    protected override async Task OnMessageActivityAsync(
        ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Message from {UserId}: {Text}",
            turnContext.Activity.From?.Id,
            turnContext.Activity.Text);

        // Typing indicator before processing.
        await turnContext.SendActivityAsync(
            new Activity { Type = ActivityTypes.Typing },
            cancellationToken);

        var conversationData = await _conversationAccessor.GetAsync(
            turnContext, () => new ConversationData(), cancellationToken);
        conversationData.MessageCount++;
        conversationData.LastInteractionUtc = DateTime.UtcNow;
        await _conversationAccessor.SetAsync(turnContext, conversationData, cancellationToken);

        var userProfile = await _userAccessor.GetAsync(
            turnContext, () => new UserProfile(), cancellationToken);

        var dialogSet = new DialogSet(_dialogStateAccessor);
        dialogSet.Add(_mainDialog);

        var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
        var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

        if (dialogResult.Status == DialogTurnStatus.Empty)
        {
            if (!userProfile.OnboardingComplete)
            {
                await dialogContext.BeginDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
            else
            {
                await HandleUserIntentAsync(turnContext, userProfile, conversationData, cancellationToken);
            }
        }
    }

    private async Task HandleUserIntentAsync(
        ITurnContext turnContext,
        UserProfile userProfile,
        ConversationData conversationData,
        CancellationToken cancellationToken)
    {
        var text = turnContext.Activity.Text?.Trim() ?? string.Empty;
        var name = userProfile.Name
            ?? conversationData.UserName
            ?? "there";

        if (IsGreeting(text))
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text($"Hello again, **{name}**! How can I help you today?"),
                cancellationToken);
            return;
        }

        if (TryParseWeatherQuery(text, out var city))
        {
            await HandleWeatherAsync(turnContext, city, cancellationToken);
            return;
        }

        if (_faqService.TryGetAnswer(text, out var faqAnswer))
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(faqAnswer), cancellationToken);
            return;
        }

        _logger.LogInformation("Fallback for unrecognized message: {Text}", text);
        await turnContext.SendActivityAsync(
            MessageFactory.Text(
                $"I'm not sure how to answer that, **{name}**.\n\n" +
                "Type **help** to see what I can do, or try **hours**, **pricing**, or **weather London**."),
            cancellationToken);
    }

    private async Task HandleWeatherAsync(
        ITurnContext turnContext,
        string city,
        CancellationToken cancellationToken)
    {
        try
        {
            var weatherText = await _weatherService.GetWeatherTextAsync(city, cancellationToken);

            if (weatherText is null)
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"I couldn't find weather data for **{city}**. Please check the city name."),
                    cancellationToken);
                return;
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(weatherText), cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Weather API not configured");
            await turnContext.SendActivityAsync(
                MessageFactory.Text("Weather service is not configured. Add WeatherApi:ApiKey in appsettings.json."),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weather lookup failed for {City}", city);
            await turnContext.SendActivityAsync(
                MessageFactory.Text("Sorry, I couldn't retrieve the weather right now. Please try again later."),
                cancellationToken);
        }
    }

    private static bool IsGreeting(string text) =>
        text.Equals("hi", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("hello", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("hey", StringComparison.OrdinalIgnoreCase);

    private static bool TryParseWeatherQuery(string text, out string city)
    {
        city = string.Empty;
        const string prefix = "weather";

        if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = text[prefix.Length..].Trim();
        if (remainder.StartsWith("in ", StringComparison.OrdinalIgnoreCase))
        {
            remainder = remainder[3..].Trim();
        }

        if (string.IsNullOrWhiteSpace(remainder))
        {
            return false;
        }

        city = remainder;
        return true;
    }

    /// <summary>Save ConversationState and UserState at end of every turn.</summary>
    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }
}
