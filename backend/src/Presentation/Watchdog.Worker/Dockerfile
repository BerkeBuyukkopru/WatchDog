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