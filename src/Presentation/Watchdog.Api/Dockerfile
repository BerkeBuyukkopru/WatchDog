# 1. Aşama: Çalışma Ortamı (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Aşama: Derleme (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# ÖNCE: Bağımlılıkları (csproj dosyalarını) kopyalıyoruz. 
# Bu sayede kod değişse bile kütüphane indirme aşaması Docker önbelleğinden (cache) hızlıca geçer.
COPY ["src/Presentation/Watchdog.Api/Watchdog.Api.csproj", "src/Presentation/Watchdog.Api/"]
COPY ["src/Core/Watchdog.Domain/Watchdog.Domain.csproj", "src/Core/Watchdog.Domain/"]
COPY ["src/Core/Watchdog.Application/Watchdog.Application.csproj", "src/Core/Watchdog.Application/"]
COPY ["src/Infrastructure/Watchdog.Infrastructure/Watchdog.Infrastructure.csproj", "src/Infrastructure/Watchdog.Infrastructure/"]

# NuGet Paketlerini Geri Yükle
RUN dotnet restore "src/Presentation/Watchdog.Api/Watchdog.Api.csproj"

# Tüm Kaynak Kodları Kopyala
COPY . .
WORKDIR "/src/src/Presentation/Watchdog.Api"
RUN dotnet build "Watchdog.Api.csproj" -c Release -o /app/build

# 3. Aşama: Yayınlama
FROM build AS publish
RUN dotnet publish "Watchdog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Aşama: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Watchdog.Api.dll"]