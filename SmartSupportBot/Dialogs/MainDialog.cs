using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SmartSupportBot.Models;

namespace SmartSupportBot.Dialogs;

/// <summary>
/// Waterfall Dialog that collects the user's name across multiple turns.
/// Persists the name to both UserState and ConversationState.
/// </summary>
public class MainDialog : ComponentDialog
{
    private const string NamePrompt = "NamePrompt";

    private readonly UserState _userState;
    private readonly ConversationState _conversationState;
    private readonly IStatePropertyAccessor<UserProfile> _userAccessor;
    private readonly IStatePropertyAccessor<ConversationData> _conversationAccessor;

    public MainDialog(UserState userState, ConversationState conversationState)
        : base(nameof(MainDialog))
    {
        _userState = userState;
        _conversationState = conversationState;
        _userAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));
        _conversationAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));

        AddDialog(new TextPrompt(NamePrompt, ValidateNameAsync));
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            IntroAndAskNameAsync,
            SaveNameAndGreetAsync
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    /// <summary>Waterfall step 1 — greet and prompt for name.</summary>
    private static async Task<DialogTurnResult> IntroAndAskNameAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var promptOptions = new PromptOptions
        {
            Prompt = MessageFactory.Text(
                "Welcome to **Smart Support Bot**! 👋\n\n" +
                "I'm here to help with FAQs, weather, and more.\n" +
                "What should I call you?")
        };

        return await stepContext.PromptAsync(NamePrompt, promptOptions, cancellationToken);
    }

    /// <summary>Waterfall step 2 — save name to UserState + ConversationState.</summary>
    private async Task<DialogTurnResult> SaveNameAndGreetAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var name = (string)stepContext.Result;

        var profile = await _userAccessor.GetAsync(
            stepContext.Context, () => new UserProfile(), cancellationToken);
        profile.Name = name;
        profile.OnboardingComplete = true;
        await _userAccessor.SetAsync(stepContext.Context, profile, cancellationToken);

        var conversation = await _conversationAccessor.GetAsync(
            stepContext.Context, () => new ConversationData(), cancellationToken);
        conversation.UserName = name;
        await _conversationAccessor.SetAsync(stepContext.Context, conversation, cancellationToken);

        await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
        await _conversationState.SaveChangesAsync(stepContext.Context, false, cancellationToken);

        await stepContext.Context.SendActivityAsync(
            MessageFactory.Text(
                $"Great to meet you, **{name}**! I've saved your name.\n\n" +
                "Type **help** to see what I can do."),
            cancellationToken);

        return await stepContext.EndDialogAsync(name, cancellationToken);
    }

    private static async Task<bool> ValidateNameAsync(
        PromptValidatorContext<string> promptContext,
        CancellationToken cancellationToken)
    {
        var name = promptContext.Recognized.Value?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            await promptContext.Context.SendActivityAsync(
                MessageFactory.Text("Please enter a valid name (at least 2 characters)."),
                cancellationToken);
            return false;
        }

        return true;
    }
}
