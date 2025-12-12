# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY impojuego.sln ./
COPY ./impojuego/ ./impojuego/
COPY ./ImpoJuego.Api/ ./ImpoJuego.Api/
COPY ./ImpoJuego.Tests/ ./ImpoJuego.Tests/

RUN dotnet restore
RUN dotnet publish ImpoJuego.Api/ImpoJuego.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "ImpoJuego.Api.dll"]
