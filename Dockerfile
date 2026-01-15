FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (simpler approach for Docker build)
COPY . .

# Restore dependencies
WORKDIR /src
RUN dotnet restore streaming-service.sln

# Build and publish
WORKDIR /src/src/streaming-service.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "streaming-service.Api.dll"]
