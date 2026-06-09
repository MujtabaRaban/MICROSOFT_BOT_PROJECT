using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace SmartSupportBot.Adapters;

/// <summary>
/// Cloud adapter subclass that centralizes error handling via <see cref="OnTurnError"/>.
/// Sends a user-friendly message on failure and logs the exception.
/// In the Emulator, trace activities are also emitted for debugging.
/// </summary>
public class AdapterWithErrorHandler : CloudAdapter
{
    public AdapterWithErrorHandler(
        BotFrameworkAuthentication auth,
        ILogger<AdapterWithErrorHandler> logger)
        : base(auth, logger)
    {
        OnTurnError = async (turnContext, exception) =>
        {
            // Log every failure — include conversation ID for correlation.
            logger.LogError(
                exception,
                "[OnTurnError] unhandled error in conversation {ConversationId}",
                turnContext.Activity.Conversation?.Id);

            // Send a friendly message so the user is not left hanging.
            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    "Sorry, something went wrong on my end. Please try again in a moment."),
                cancellationToken: default);

            // Emulator-only: emit a trace activity developers can inspect.
            if (turnContext.Activity.ChannelId == "emulator")
            {
                await turnContext.TraceActivityAsync(
                    "OnTurnError Trace",
                    exception.Message,
                    "https://www.botframework.com/schemas/error",
                    "TurnError");
            }
        };
    }
}
