# Release Notes — v1.2.0

**Release date:** 2026-08-06

---

## Overview

SearchEngine Backend API **v1.2.0** adds a full **Health Checks dashboard** and hardens operational observability. It introduces a visual Health Checks UI, service-level checks (databases, background jobs, email, file storage), a machine-readable JSON endpoint for orchestrator probes, and protects the operational dashboards (Hangfire + Health Checks UI) behind authentication.

This release contains **breaking changes** to the health endpoint. See [Breaking Changes](#breaking-changes) and the [Upgrade Guide](#upgrade-guide).

---

## Highlights

- Visual **Health Checks UI** dashboard at `/healthcheck-ui` with polling and history.
- Service-level checks with friendly names and tags: 3 databases, Hangfire service, SMTP, and file storage.
- Machine-readable `/healthcheck-json` for Docker/Kubernetes probes and monitoring.
- Operational dashboards (`/hangfire`, `/healthcheck-ui`) protected with **HTTP Basic Auth** validated against ASP.NET Identity (SuperAdmin).
- Critical vs non-critical **tiering**: databases and file storage fail as `Unhealthy` (503); Hangfire and SMTP fail as `Degraded` (still 200).
- No vulnerable dependencies — transitive advisories resolved by pinning.

---

## Breaking Changes

| Area | Before | After |
| ---- | ------ | ----- |
| Health endpoint | `/health` (raw JSON) | **Removed.** Use `/healthcheck-json` (JSON, for machines) and `/healthcheck-ui` (dashboard, for humans). |
| Hangfire dashboard | Open in Development | **Requires Basic Auth (SuperAdmin)** in all environments. |

---

## Endpoints

| Method | Route | Purpose | Auth |
| ------ | ----- | ------- | ---- |
| `GET` | `/healthcheck-ui` | Visual dashboard (humans) | Basic Auth (SuperAdmin) |
| `GET` | `/healthcheck-json` | Machine JSON (probes/monitoring) + dashboard data source | Open |
| `GET` | `/hangfire` | Background job dashboard | Basic Auth (SuperAdmin) |
| `GET` | `/healthcheck-api`, `/healthcheck-resources` | Internal dashboard SPA (data + assets) | mixed |

---

## Health Checks

Six checks are reported on the dashboard:

| Check | Tag | Failure status |
| ----- | --- | -------------- |
| Application Database: SQL Server | `Database` | Unhealthy (critical) |
| Identity Database: SQL Server | `Database` | Unhealthy (critical) |
| Background Job Database: SQL Server (Hangfire) | `Database` | Unhealthy (critical) |
| Background Job Service: Hangfire | `Background Job` | Degraded |
| Email Service: SMTP | `Email` | Degraded |
| File Storage Service: File System | `File Storage` | Unhealthy (critical) |

- **Tiering rationale:** databases and writable storage are required for the app to function → `Unhealthy` → `/healthcheck-json` returns `503`, so orchestrator probes correctly mark the instance down. Background jobs and email are non-critical → `Degraded` → endpoint still returns `200`.
- The SMTP and File Storage checks are custom (no extra packages), keeping the dependency surface clean.

---

## Dashboard Authentication

The Hangfire and Health Checks UI dashboards are gated by a Basic Auth middleware:

- The browser shows its native username/password prompt.
- Credentials are validated against **ASP.NET Identity**, and only users in the **SuperAdmin** role are allowed.
- `/healthcheck-json` stays **open** so machine probes are not blocked.
- **HTTPS is required in production** (Basic Auth transmits credentials on every request; the template already enables HTTPS redirection).

---

## Dependencies

Added (all restored from the standard feed, no vulnerabilities):

- `AspNetCore.HealthChecks.UI`, `AspNetCore.HealthChecks.UI.Client`, `AspNetCore.HealthChecks.UI.SqlServer.Storage`
- `AspNetCore.HealthChecks.Hangfire`

Pinned to resolve transitive security advisories pulled in by the UI package:

- `KubernetesClient` → `18.0.13` (resolves NU1903 from `15.0.1`)
- `IdentityModel` → `5.2.0` (assembly required by the UI collector but not otherwise deployed)

---

## Configuration

The Health Checks UI collector polls an absolute URL (a background collector has no request context to resolve a relative one):

```json
"HealthChecksUI": {
  "HealthCheckEndpoint": "http://127.0.0.1:5152/healthcheck-json"
}
```

Override this per environment. The UI stores polling history in SQL Server (the Hangfire connection); its tables are created automatically on startup.

---

## Upgrade Guide

1. Point monitoring/uptime tools and Docker/Kubernetes probes from `/health` to **`/healthcheck-json`**.
2. Grant the **SuperAdmin** role to whoever needs the dashboards; they log in via the browser popup at `/healthcheck-ui` and `/hangfire`.
3. If you deploy on a non-default port/host, set `HealthChecksUI:HealthCheckEndpoint` accordingly.
4. No manual database migration is required — the Health Checks UI storage tables are created on startup.

---

## Verification

- Solution builds clean (`0 warnings, 0 errors`).
- Full test suite passes (unit, domain, and integration).

```bash
dotnet build SearchEngine.slnx
dotnet test SearchEngine.slnx
```

---

## Full Changelog

See [../CHANGELOG.md](../CHANGELOG.md) for the complete list of changes in this release.
