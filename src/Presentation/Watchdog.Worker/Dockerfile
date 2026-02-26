# 1. Aşama: Çalışma Ortamı (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# 2. Aşama: Derleme (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# KLASÖR YOLLARI DÜZELTİLDİ: Ekran görüntündeki hiyerarşiye göre (1.Core değil, Core)
COPY ["src/Presentation/Watchdog.Worker/Watchdog.Worker.csproj", "src/Presentation/Watchdog.Worker/"]
COPY ["src/Core/Watchdog.Domain/Watchdog.Domain.csproj", "src/Core/Watchdog.Domain/"]
COPY ["src/Core/Watchdog.Application/Watchdog.Application.csproj", "src/Core/Watchdog.Application/"]
COPY ["src/Infrastructure/Watchdog.Infrastructure/Watchdog.Infrastructure.csproj", "src/Infrastructure/Watchdog.Infrastructure/"]

# Kütüphaneleri indir (Restore)
RUN dotnet restore "src/Presentation/Watchdog.Worker/Watchdog.Worker.csproj"

# SONRA: Kalan tüm dosyaları kopyala
COPY . .
WORKDIR "/src/src/Presentation/Watchdog.Worker"
RUN dotnet build "Watchdog.Worker.csproj" -c Release -o /app/build

# 3. Aşama: Yayınlama (Publish)
FROM build AS publish
RUN dotnet publish "Watchdog.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Aşama: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Watchdog.Worker.dll"]