FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY streaming-service.sln ./
COPY src/streaming-service.Domain/*.csproj ./src/streaming-service.Domain/
COPY src/streaming-service.Application/*.csproj ./src/streaming-service.Application/
COPY src/streaming-service.Infrastructure/*.csproj ./src/streaming-service.Infrastructure/
COPY src/streaming-service.Api/*.csproj ./src/streaming-service.Api/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . ./
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
