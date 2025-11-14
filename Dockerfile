# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Puerto requerido por Cloud Run
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /out .

ENTRYPOINT ["dotnet", "WebColegio.dll"]
