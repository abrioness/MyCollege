# Usar imagen base de .NET SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copiar archivos del proyecto
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Imagen runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out ./

# Exponer el puerto
EXPOSE 80

ENTRYPOINT ["dotnet", "WebColegio.dll"]
