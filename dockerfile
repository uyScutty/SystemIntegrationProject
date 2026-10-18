FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["packages/Shared.Contracts/Shared.Contracts.csproj", "packages/Shared.Contracts/"]
COPY ["src/VoiceVideoService/VoiceVideoService.csproj", "src/VoiceVideoService/"]
RUN dotnet restore "src/VoiceVideoService/VoiceVideoService.csproj"

COPY . .
WORKDIR /src/src/VoiceVideoService
RUN dotnet build "VoiceVideoService.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/build \
    --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VoiceVideoService.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoiceVideoService.dll"]
