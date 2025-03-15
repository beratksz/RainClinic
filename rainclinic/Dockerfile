# 1️ Taban imajı (AspNet Core Çalışma Zamanı)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081

# 2️ Build ve Restore (SDK Kullanan Katman)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["rainclinic/rainclinic.csproj", "./"]
RUN dotnet restore "rainclinic.csproj"

# 3️ Tüm dosyaları kopyala ve derleme işlemi yap
COPY . .
WORKDIR "/src/rainclinic"
RUN dotnet build "rainclinic.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 4️ Uygulamayı yayına hazır hale getir (Publish)
FROM build AS publish
RUN dotnet publish "rainclinic.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 5️ Final Katman: Production İçin Optimize Edilmiş
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 6️ Çevre Değişkenlerini Ayarla
ENV ASPNETCORE_URLS=http://+:8081
ENV DOTNET_RUNNING_IN_CONTAINER=true

# 7️ Uygulamayı Başlat
ENTRYPOINT ["dotnet", "rainclinic.dll"]
