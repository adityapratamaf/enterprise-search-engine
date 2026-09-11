# Release Notes — v1.3.0

**Release date:** 2026-09-08

---

## Overview

SearchEngine Backend API **v1.3.0** is a **refinement and hardening** release. It tightens JWT token validation, moves exception handling onto the framework-native pipeline, makes offset pagination deterministic, and stabilises the CI formatting gate across Windows and Linux.

There are **no breaking API changes** — request/response contracts, authentication claims, and JWT payload semantics are unchanged. See [Notable Changes](#notable-changes) for two behavioural refinements and the [Upgrade Guide](#upgrade-guide).

---

## Highlights

- **Tighter JWT grace period** — token `ClockSkew` reduced from the 5-minute default to **30 seconds**.
- **Framework-native exception handling** — the custom error middleware is replaced by ASP.NET Core `IExceptionHandler`, with the existing error envelope preserved.
- **Deterministic pagination** — a stable `Id` tie-breaker prevents rows from being skipped or duplicated across page boundaries.
- **Supporting index** — a filtered index on `Products(Name)` backs the default product listing.
- **Reliable CI formatting** — a `.gitattributes` normalises line endings so `dotnet format` behaves identically on developer machines and CI runners.
- Test suite grown from **44 → 54**, all passing. No vulnerable dependencies.

---

## Notable Changes

| Area | Before | After |
| ---- | ------ | ----- |
| JWT ClockSkew | 5 minutes (default) — expired tokens usable up to 5 min longer | **30 seconds** — tokens expire close to their real lifetime |
| List ordering | Non-unique sort columns (e.g. Name, CreatedAt) could reorder ties, causing skip/duplicate at page boundaries | **Stable order** via `Id` tie-breaker; a given result set now paginates consistently |

Neither change alters an endpoint route, request schema, or response schema. Clients relying on a 5-minute token grace should ensure host clocks are reasonably synchronised (30 s tolerance remains).

---

## Details

### A-015 — JWT ClockSkew
The JwtBearer `TokenValidationParameters.ClockSkew` is now explicitly `TimeSpan.FromSeconds(30)`. Signing algorithm, secret/key, issuer, audience, token generation, refresh-token rotation/hashing/reuse-detection, RBAC claims, and the authentication scheme are all unchanged.

### A-016 — Centralized Exception Handling
Exception-to-HTTP mapping now uses the built-in `IExceptionHandler` / `UseExceptionHandler` pipeline instead of a custom middleware. The existing `Result<T>` error envelope is preserved, so the public error contract is identical. Mapping:

| Exception | Status |
| --------- | ------ |
| Validation | `400` |
| Unauthorized | `401` |
| Forbidden | `403` |
| Not Found | `404` |
| Conflict | `409` |
| Unexpected | `500` |

Unexpected exceptions are logged server-side with method/path; the client receives only a generic message — no stack traces, database errors, connection strings, or secrets are exposed.

### A-017 — Formatting Consistency
Line endings are normalised via `.gitattributes` (CRLF, matching `.editorconfig`), so `dotnet format SearchEngine.slnx --verify-no-changes` is deterministic across platforms and the CI quality gate no longer produces false failures. No CI workflow changes were required.

### A-018 — Pagination & Search
- **Deterministic pagination:** paginated queries (Products, Audit actions, Users) now always append a stable `Id` tie-breaker to the sort, eliminating skip/duplicate at page boundaries.
- **Index:** a filtered index `IX_Products_Name` on `Products(Name) WHERE [IsDeleted] = 0` supports the default listing and prefix searches while staying small.
- **Offset pagination and substring search are intentionally retained** — keyset pagination was not adopted (no endpoint yet justifies it, and the page-number contract is public), and substring search semantics are unchanged. All search parameters remain parameterised.

---

## Migrations

This release adds one EF Core migration on the business context:

- **`AddProductNameIndex`** — creates the filtered index on `Products(Name)` and aligns the `Name` column to `nvarchar(200)` (the value the model already declares).

The migration is applied automatically in Development / when `Database:MigrateOnStartup=true` or the migration job runs. It is safe on existing databases (the column change is a no-op where already bounded).

> **Note:** while adding the index we found that the legacy `AddSearchEngineConstraints` migration ships without a `.Designer.cs`/`[Migration]` attribute and is therefore silently skipped by EF, leaving some intended column constraints unapplied. The new migration neutralises only the part it needs (`Products.Name`). A full reconciliation of that legacy migration is tracked for a future release.

---

## Upgrade Guide

1. No API, claim, or response-schema changes — clients require no updates.
2. Apply migrations (`AddProductNameIndex`) — automatic on startup in Development, or run the migration job in production.
3. Ensure server clocks are reasonably synchronised, given the tighter 30-second token skew.

---

## Verification

- Solution builds clean (`0 warnings, 0 errors`).
- Full test suite passes — **54/54** (unit, domain, integration), up from 44.
- `dotnet format --verify-no-changes`: clean. No vulnerable packages.

```bash
dotnet build SearchEngine.slnx
dotnet test SearchEngine.slnx
dotnet format SearchEngine.slnx --verify-no-changes
dotnet list SearchEngine.slnx package --vulnerable
```

---

## Full Changelog

See [../CHANGELOG.md](../CHANGELOG.md) for the complete list of changes in this release.
