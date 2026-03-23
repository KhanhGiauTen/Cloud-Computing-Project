# Multi-stage Dockerfile for CloudContactManager backend (.NET 8)

# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY CloudContactManager/CloudContactManager.csproj CloudContactManager/
COPY CloudContactManager.Tests/CloudContactManager.Tests.csproj CloudContactManager.Tests/
RUN dotnet restore CloudContactManager/CloudContactManager.csproj

# Copy all source code
COPY . .

# Publish backend app
RUN dotnet publish CloudContactManager/CloudContactManager.csproj -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Optional: set container listen URL
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CloudContactManager.dll"]
