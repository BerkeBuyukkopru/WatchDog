FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# src katmanlarını kopyala
COPY ["backend/src/Presentation/Watchdog.Api/Watchdog.Api.csproj", "backend/src/Presentation/Watchdog.Api/"]
COPY ["backend/src/Core/Watchdog.Domain/Watchdog.Domain.csproj", "backend/src/Core/Watchdog.Domain/"]
COPY ["backend/src/Core/Watchdog.Application/Watchdog.Application.csproj", "backend/src/Core/Watchdog.Application/"]
COPY ["backend/src/Infrastructure/Watchdog.Infrastructure/Watchdog.Infrastructure.csproj", "backend/src/Infrastructure/Watchdog.Infrastructure/"]

# BAĞIMLILIK ÇÖZÜMÜ: modules katmanlarını kopyala (Infra bunlara muhtaç)
COPY ["backend/modules/HealthChecks.Abstractions/HealthChecks.Abstractions.csproj", "backend/modules/HealthChecks.Abstractions/"]
COPY ["backend/modules/HealthChecks.System/HealthChecks.System.csproj", "backend/modules/HealthChecks.System/"]

RUN dotnet restore "backend/src/Presentation/Watchdog.Api/Watchdog.Api.csproj"

COPY . .
WORKDIR "/src/backend/src/Presentation/Watchdog.Api"
RUN dotnet build "Watchdog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Watchdog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Watchdog.Api.dll"]