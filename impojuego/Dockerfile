# Build stage — SDK 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Solo se copian los proyectos necesarios para el publish del API.
# (ImpoJuego.Tests queda fuera a propósito: targetea net9.0 y SDK 8.0
#  no puede restaurarlo; además no se necesita para el runtime.)
COPY ./impojuego/ ./impojuego/
COPY ./ImpoJuego.Api/ ./ImpoJuego.Api/

# Restore y publish solo el API
RUN dotnet restore ImpoJuego.Api/ImpoJuego.Api.csproj
RUN dotnet publish ImpoJuego.Api/ImpoJuego.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# Runtime stage — ASP.NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render asigna PORT dinámico; la app lo lee en Program.cs
# EXPOSE es informativo, no ata el container a ningún puerto.
EXPOSE 8080

ENTRYPOINT ["dotnet", "ImpoJuego.Api.dll"]
