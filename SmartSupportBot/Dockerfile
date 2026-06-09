# ---------------------------------------------------------------------------
# SmartSupportBot — production container image for Azure App Service / AKS
# ---------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3978
ENV ASPNETCORE_URLS=http://+:3978
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SmartSupportBot/SmartSupportBot.csproj", "SmartSupportBot/"]
RUN dotnet restore "SmartSupportBot/SmartSupportBot.csproj"
COPY . .
WORKDIR "/src/SmartSupportBot"
RUN dotnet build "SmartSupportBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SmartSupportBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartSupportBot.dll"]
