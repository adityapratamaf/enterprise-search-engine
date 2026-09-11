# Setup Guide

This document explains how to run the SearchEngine Backend API in both Local Development and Docker environments.

---

# Requirements

## Local Development

* .NET 10 SDK
* SQL Server 2022+
* Visual Studio / VS Code / Rider

---

## Docker

* Docker Desktop
* Docker Compose

---

# Clone Repository

```bash
git clone <repository-url>

cd SearchEngine
```

---

# Configuration

Configure the required settings inside:

```text
src/Presentation/SearchEngine.WebAPI/appsettings.Development.json
```

or

```text
src/Presentation/SearchEngine.WebAPI/appsettings.Docker.json
```

Example

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "...",
    "BusinessConnection": "...",
    "HangfireConnection": "..."
  },

  "JwtSettings": {
    "Secret": "...",
    "Issuer": "SearchEngine",
    "Audience": "SearchEngineUsers"
  }
}
```

---

# Local Development

## Restore Packages

```bash
dotnet restore
```

---

## Build

```bash
dotnet build
```

---

## Run Application

```bash
dotnet run --project src/Presentation/SearchEngine.WebAPI
```

---

# Application URLs (Local)

After running the application, the following endpoints are available:

| URL                                    | Description                 |
| -------------------------------------- | --------------------------- |
| http://localhost:5152/scalar/docs      | API Documentation (Scalar)  |
| http://localhost:5152/healthcheck-ui   | Health Check UI (dashboard) |
| http://localhost:5152/healthcheck-json | Health Check JSON           |
| http://localhost:5152/hangfire         | Background Job (Hangfire)   |

---

# Database

This project uses two SQL Server databases.

| Database             | Purpose                        |
| -------------------- | ------------------------------ |
| SearchEngineIdentityDB | Authentication & Authorization |
| SearchEngineBusinessDB | Business Data                  |
| SearchEngineHangfireDB | Hangfire Background Job        |

---

# Entity Framework Migration

## Identity Database

Create Migration

```bash
dotnet ef migrations add InitialSetupIdentity  `
--context ApplicationIdentityDbContext `
--project src/Infrastructure/SearchEngine.Infrastructure.Identity `
--startup-project src/Presentation/SearchEngine.WebAPI
```

Apply Migration

```bash
dotnet ef database update `
--context ApplicationIdentityDbContext `
--project src/Infrastructure/SearchEngine.Infrastructure.Identity `
--startup-project src/Presentation/SearchEngine.WebAPI
```

---

## Business Database

Create Migration

```bash
dotnet ef migrations add InitialSetupBusiness `
--context ApplicationBusinessDbContext `
--project src/Infrastructure/SearchEngine.Infrastructure.Persistence `
--startup-project src/Presentation/SearchEngine.WebAPI
```

Apply Migration

```bash
dotnet ef database update `
--context ApplicationBusinessDbContext `
--project src/Infrastructure/SearchEngine.Infrastructure.Persistence `
--startup-project src/Presentation/SearchEngine.WebAPI
```

---

# Automatic Migration & Seeding

When the application starts, it automatically:

* Applies pending Identity migrations
* Applies pending Business migrations
* Seeds default roles
* Seeds default modules
* Seeds default permissions
* Creates the default administrator account

No manual seeding is required.

---

# Default Administrator

```text
Email:
admin@searchengine.local

Password:
Admin123!
```

---

# API Documentation

Scalar

```text
http://localhost:5152/scalar/docs
```

OpenAPI

```text
http://localhost:5152/openapi/v1.json
```

---

# Health Check

UI Dashboard

```text
http://localhost:5152/healthcheck-ui
```

JSON (for monitoring, Docker/Kubernetes probes)

```text
http://localhost:5152/healthcheck-json
```

---

# Logging

Logs are written to:

* Console
* Rolling File
* Seq

Seq Docker Install
```
docker run -d `
--name seq `
-e ACCEPT_EULA=Y `
-e SEQ_FIRSTRUN_ADMINPASSWORD=Admin123! `
-p 5341:80 `
datalust/seq
```

Seq Dashboard

```text
http://localhost:5341
```

---

# Hangfire Dashboard

If enabled:

```text
http://localhost:5152/hangfire
```

---

# Run with Docker

Build containers

```bash
docker compose build
```

Start containers

```bash
docker compose up -d
```

Stop containers

```bash
docker compose down
```

Stop and remove volumes

```bash
docker compose down -v
```

Remove network, containers, images, and volumes

```bash
docker compose down --rmi all -v --remove-orphans
```

---

# Docker Services

| Service    | Port |
| ---------- | ---- |
| API        | 5000 |
| SQL Server | 1433 |
| Seq        | 5341 |

---

# Docker URLs

API

```text
http://localhost:5000
```

Scalar

```text
http://localhost:5000/scalar/docs
```

Health Check UI

```text
http://localhost:5000/healthcheck-ui
```

Health Check JSON

```text
http://localhost:5000/healthcheck-json
```

Seq

```text
http://localhost:5341
```

---

# Testing

Run all tests

```bash
dotnet test
```

Run Unit Tests

```bash
dotnet test tests/SearchEngine.Application.UnitTests
```

Run Integration Tests

```bash
dotnet test tests/SearchEngine.Application.IntegrationTests
```

---

# Common Commands

Restore

```bash
dotnet restore SearchEngine.slnx
```

Build

```bash
dotnet build SearchEngine.slnx
```

Run

```bash
dotnet run --project src/Presentation/SearchEngine.WebAPI
```

Clean

```bash
dotnet clean SearchEngine.slnx
```

Format

```bash
dotnet format
```

---

# Troubleshooting

## Migration Not Applied

```bash
dotnet ef database update
```

---

## Remove Last Migration

```bash
dotnet ef migrations remove
```

---

## Rebuild Docker

```bash
docker compose down -v

docker compose build --no-cache

docker compose up
```
