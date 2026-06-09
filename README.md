# SmartSupportBot

Full working **Microsoft Bot Framework SDK v4** chatbot on **.NET 8**.

## Project Structure

```
SmartSupportBot/
├── Adapters/AdapterWithErrorHandler.cs
├── Bots/SupportBot.cs
├── Controllers/BotController.cs
├── Dialogs/MainDialog.cs
├── Models/
│   ├── ConversationData.cs
│   ├── OpenWeatherResponse.cs
│   ├── UserProfile.cs
│   └── WeatherApiOptions.cs
├── Services/
│   ├── FaqService.cs
│   ├── IWeatherService.cs
│   └── WeatherService.cs
├── Properties/launchSettings.json
├── Program.cs
├── appsettings.json
└── SmartSupportBot.csproj
```

## Step-by-Step Setup

### 1. Install prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Bot Framework Emulator](https://github.com/microsoft/BotFramework-Emulator/releases)
- [OpenWeatherMap API key](https://openweathermap.org/api) (free)

### 2. Configure weather API key

```powershell
cd C:\Users\BeTa\MICROSOFT_BOT_PROJECT\SmartSupportBot
dotnet user-secrets init
dotnet user-secrets set "WeatherApi:ApiKey" "your-openweathermap-api-key"
```

Or edit `appsettings.json` → `WeatherApi:ApiKey`.

### 3. Restore and build

```powershell
cd C:\Users\BeTa\MICROSOFT_BOT_PROJECT
dotnet restore SmartSupportBot.sln
dotnet build SmartSupportBot.sln
```

### 4. Run the bot

```powershell
dotnet run --project SmartSupportBot
```

Expected output: `Now listening on: http://localhost:3978`

---

## Bot Framework Emulator

1. Start the bot (`dotnet run --project SmartSupportBot`)
2. Open **Bot Framework Emulator**
3. Click **Open Bot** and enter:

| Field | Value |
|-------|-------|
| Bot URL | `http://localhost:3978/api/messages` |
| Microsoft App ID | *(leave blank)* |
| Microsoft App Password | *(leave blank)* |

4. Click **Connect**

### Test conversation

| You type | Bot does |
|----------|----------|
| *(first message)* | Asks for your name (Waterfall Dialog) |
| `Alex` | Saves name to UserState + ConversationState |
| `help` | Shows command list |
| `hours` | FAQ answer |
| `weather London` | Formatted weather text |
| `xyz123` | Fallback response |

---

## Common Errors and Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `Connection refused` / Emulator can't connect | Bot not running | Run `dotnet run --project SmartSupportBot` |
| `404 Not Found` on `/api/messages` | Wrong URL or port | Use exactly `http://localhost:3978/api/messages` |
| `401 Unauthorized` | App ID/Password mismatch | Leave both **blank** for local Emulator |
| `Weather API key is not configured` | Missing API key | Set `WeatherApi:ApiKey` in appsettings or user-secrets |
| `City not found` | Invalid city name | Try `weather Paris` or `weather New York` |
| `dotnet` not recognized | .NET SDK not installed | Install .NET 8 SDK and restart terminal |
| Dialog keeps asking for name | State reset | In Emulator: **Restart Conversation** |
| Port 3978 already in use | Another process on port | Stop other app or change port in `launchSettings.json` |

---

## License

MIT
