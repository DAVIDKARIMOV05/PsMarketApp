# 1. Aşama: İnşaat (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Dosyaları kopyala
COPY . .

# Paketleri yükle ve derle
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2. Aşama: Çalıştırma (Run)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "PsMarketApp.dll"]