# ============================
# Etapa 1: Build
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todos los archivos del proyecto
COPY . .

# Restaurar dependencias
RUN dotnet restore

# Publicar en modo Release
RUN dotnet publish -c Release -o /app

# ============================
# Etapa 2: Runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Puerto para Cloud Run
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copiar archivos publicados
COPY --from=build /app .

# Iniciar app
ENTRYPOINT ["dotnet", "WebColegio.dll"]
