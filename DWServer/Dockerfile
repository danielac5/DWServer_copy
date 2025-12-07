# Imagine de bază pentru .NET 8 SDK (pentru build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiem csproj-ul proiectului server și restaurăm pachetele
COPY DWServer/DWServer.csproj ./DWServer/
RUN dotnet restore ./DWServer/DWServer.csproj

# Copiem tot codul sursă și facem build
COPY . ./
RUN dotnet publish DWServer/DWServer.csproj -c Release -o out

# Imagine de rulare
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expunem portul aplicației
EXPOSE 5000

# Comanda de start
ENTRYPOINT ["dotnet", "DWServer.dll"]
