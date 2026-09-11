# ============================
# BUILD
# ============================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
src/Presentation/SearchEngine.WebAPI/SearchEngine.WebAPI.csproj

RUN dotnet publish \
src/Presentation/SearchEngine.WebAPI/SearchEngine.WebAPI.csproj \
-c Release \
-o /app/publish \
--no-restore

# ============================
# RUNTIME
# ============================

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# curl is used by the Docker HEALTHCHECK below
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

# Writable directories for logs and file storage, owned by the non-root
# 'app' user that ships in the .NET runtime image.
RUN mkdir -p /app/logs /app/storage \
    && chown -R app:app /app/logs /app/storage

# Run as non-root
USER app

EXPOSE 8080

# Container health probe against the unauthenticated health endpoint.
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/healthcheck-json || exit 1

ENTRYPOINT ["dotnet", "SearchEngine.WebAPI.dll"]