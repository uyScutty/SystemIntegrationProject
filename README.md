# Bizcord VoiceVideoService

VoiceVideoService er Bizcords microservice for voice/video-opkald. Den ejer opkaldssessioner og deltagere og reagerer på relevante besked-events fra MessagingService.

Den detaljerede afgrænsning, dataejerskab, diagrams og arkitekturbeslutninger findes i [docs/architecture.md](docs/architecture.md).

## Teknologi

- .NET 9 og ASP.NET Core Web API
- Entity Framework Core med SQL Server
- RabbitMQ med EasyNetQ bag `IMessageClient`
- xUnit, SQLite In-Memory og `WebApplicationFactory`
- Docker og Docker Compose

## REST API

Base path er `/api/voicevideo`.

| Metode | Path | Funktion |
| --- | --- | --- |
| `GET` | `/` | Service-status |
| `POST` | `/sessions` | Opret session |
| `GET` | `/sessions` | Hent alle sessioner |
| `GET` | `/sessions/{id}` | Hent én session |
| `PUT` | `/sessions/{id}/end` | Afslut session |
| `DELETE` | `/sessions/{id}` | Slet session |

Eksempel på body til `POST /sessions`:

```json
{
  "channelId": "11111111-1111-1111-1111-111111111111"
}
```

## Lokal build og test

Fra repositoryets rod:

```powershell
dotnet restore .\tests\VoiceVideoService.Tests\VoiceVideoService.Tests.csproj
dotnet build .\tests\VoiceVideoService.Tests\VoiceVideoService.Tests.csproj --no-restore
dotnet test .\tests\VoiceVideoService.Tests\VoiceVideoService.Tests.csproj --no-build
```

Den rigtige RabbitMQ-integrationstest er opt-in. Start RabbitMQ, angiv forbindelsen og kør:

```powershell
$env:RUN_RABBITMQ_INTEGRATION_TESTS = "true"
$env:RabbitMQ = "host=localhost;username=<user>;password=<password>"
dotnet test .\tests\VoiceVideoService.Tests\VoiceVideoService.Tests.csproj --filter "FullyQualifiedName~MessagePostedEvent_IsConsumed_AndResultEventPublished"
```

Uden opt-in bliver testen markeret skipped, mens unit- og REST-testene stadig kører.

## Docker Compose

Kopiér `.env.example` til `.env`, og udskift alle placeholder-værdier. `.env` er ignoreret af Git.

```powershell
docker compose config --quiet
docker compose up --build
```

API'en bliver tilgængelig på `http://localhost:8080`, og RabbitMQ Management på `http://localhost:15672`.

Compose bruger disse konfigurationsnøgler:

- `RabbitMQ`
- `ConnectionStrings__VoiceVideoDb`

SQL Server og RabbitMQ skal være healthy, før API-containeren starter. API'en opretter et nyt databaseskema med EF Core `EnsureCreatedAsync`.

## Kendte produktionshensyn

- Brug EF Core migrations i stedet for `EnsureCreatedAsync` i en egentlig release-pipeline.
- Brug en Transactional Outbox, hvis databaseændring og eventpublicering skal være atomisk.
- Håndhæv én aktiv session pr. kanal på databaseniveau ved høj samtidighed.
- Læg secrets i platformens secret store; commit aldrig `.env`.
