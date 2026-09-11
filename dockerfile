# --- Bygge-fasen (opdateret til 9.0) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "src/VoiceVideoService/VoiceVideoService.csproj"
RUN dotnet publish "src/VoiceVideoService/VoiceVideoService.csproj" -c Release -o /app/publish

# --- Køre-fasen (opdateret til 9.0) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VoiceVideoService.dll"]