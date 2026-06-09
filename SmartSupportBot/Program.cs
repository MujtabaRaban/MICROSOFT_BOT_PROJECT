using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using SmartSupportBot.Adapters;
using SmartSupportBot.Bots;
using SmartSupportBot.Dialogs;
using SmartSupportBot.Models;
using SmartSupportBot.Services;

// ---------------------------------------------------------------------------
// Program.cs — .NET 8 minimal hosting model with Bot Framework DI wiring.
// ---------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// Bind weather API settings from appsettings.json.
builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));

// ASP.NET Core MVC for BotController.
builder.Services.AddControllers();

// Bot Framework authentication (MicrosoftAppId / Password from config).
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Cloud adapter with OnTurnError handler.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// In-memory state storage (use Cosmos/Blob storage in Azure production).
IStorage storage = new MemoryStorage();
builder.Services.AddSingleton(storage);
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<UserState>();

// Dialogs and bot.
builder.Services.AddSingleton<MainDialog>();
builder.Services.AddTransient<IBot, SupportBot>();

// Application services.
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddSingleton<FaqService>();

// Logging.
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
