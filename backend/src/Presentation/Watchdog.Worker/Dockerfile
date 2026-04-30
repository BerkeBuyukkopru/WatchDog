FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# src katmanlarını kopyala
COPY ["backend/src/Presentation/Watchdog.Worker/Watchdog.Worker.csproj", "backend/src/Presentation/Watchdog.Worker/"]
COPY ["backend/src/Core/Watchdog.Domain/Watchdog.Domain.csproj", "backend/src/Core/Watchdog.Domain/"]
COPY ["backend/src/Core/Watchdog.Application/Watchdog.Application.csproj", "backend/src/Core/Watchdog.Application/"]
COPY ["backend/src/Infrastructure/Watchdog.Infrastructure/Watchdog.Infrastructure.csproj", "backend/src/Infrastructure/Watchdog.Infrastructure/"]

# BAĞIMLILIK ÇÖZÜMÜ: modules katmanlarını kopyala
COPY ["backend/modules/HealthChecks.Abstractions/HealthChecks.Abstractions.csproj", "backend/modules/HealthChecks.Abstractions/"]
COPY ["backend/modules/HealthChecks.System/HealthChecks.System.csproj", "backend/modules/HealthChecks.System/"]
COPY ["backend/modules/HealthChecks.Heartbeat/HealthChecks.Heartbeat.csproj", "backend/modules/HealthChecks.Heartbeat/"]
COPY ["backend/modules/HealthChecks.Http/HealthChecks.Http.csproj", "backend/modules/HealthChecks.Http/"]
COPY ["backend/modules/HealthChecks.MongoDb/HealthChecks.MongoDb.csproj", "backend/modules/HealthChecks.MongoDb/"]
COPY ["backend/modules/HealthChecks.RabbitMQ/HealthChecks.RabbitMQ.csproj", "backend/modules/HealthChecks.RabbitMQ/"]
COPY ["backend/modules/HealthChecks.Redis/HealthChecks.Redis.csproj", "backend/modules/HealthChecks.Redis/"]
COPY ["backend/modules/HealthChecks.SqlServer/HealthChecks.SqlServer.csproj", "backend/modules/HealthChecks.SqlServer/"]
COPY ["backend/modules/HealthChecks.Ssl/HealthChecks.Ssl.csproj", "backend/modules/HealthChecks.Ssl/"]
COPY ["backend/modules/HealthChecks.Tcp/HealthChecks.Tcp.csproj", "backend/modules/HealthChecks.Tcp/"]

RUN dotnet restore "backend/src/Presentation/Watchdog.Worker/Watchdog.Worker.csproj"

COPY . .
WORKDIR "/src/backend/src/Presentation/Watchdog.Worker"
RUN dotnet build "Watchdog.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Watchdog.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Watchdog.Worker.dll"]