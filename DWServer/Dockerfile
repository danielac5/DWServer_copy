# Imagine de bază pentru .NET 8 SDK (pentru build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiem fișierele csproj și restaurăm pachetele
COPY *.csproj ./
RUN dotnet restore

# Copiem tot codul și facem build
COPY . ./
RUN dotnet publish -c Release -o out

# Imagine de rulare
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expunem portul aplicației
EXPOSE 5000

# Comanda de start
ENTRYPOINT ["dotnet", "DWServer.dll"]
