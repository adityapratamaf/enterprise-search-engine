# Release Notes — v1.0.0

**Release date:** 2026-07-15

---

## Overview

SearchEngine Backend API **v1.0.0** is the first stable release of a production-oriented ASP.NET Core 10 starter template. It provides a Clean Architecture foundation with authentication, fine-grained authorization, auditing, file management, background processing, and observability — designed so teams can start building business features immediately instead of assembling cross-cutting infrastructure from scratch.

This release focuses on a solid, secure, and well-structured baseline suitable for real searchengine applications.

---

## Highlights

- Clean Architecture with a strict, inward-pointing dependency direction.
- CQRS (MediatR) with a feature-oriented Application layer.
- JWT authentication with refresh token rotation and reuse detection.
- Role-Based Access Control with dynamic, permission-based authorization.
- Hardened security headers and magic-number file upload validation.
- Standardized response envelope, generic pagination, and the Result pattern.
- Structured logging, health checks, OpenAPI/Scalar documentation, and Docker Compose support.

---

## Architecture

The solution is organized into four layers with clear responsibilities:

| Layer          | Responsibility                            |
| -------------- | ----------------------------------------- |
| Presentation   | HTTP pipeline, middleware, API endpoints  |
| Application    | Business use cases, CQRS, validation      |
| Domain         | Business model and rules                  |
| Infrastructure | Database, identity, external services     |

Key architectural decisions:

- **CQRS with MediatR** — commands and queries organized by feature.
- **Result pattern (`Result<T>`)** — consistent, strongly typed outcomes without throwing for expected failures.
- **Options pattern** — typed configuration validated at startup.
- **Extension-based startup** — `Program.cs` stays minimal by delegating to composition extensions.
- **Separate infrastructure projects** — Identity, Persistence, and Shared concerns isolated.

See [Architecture.md](Architecture.md) for full details.

---

## Security Improvements

- **JWT bearer authentication** with startup-validated settings.
- **Refresh token rotation** on every use, with token chain traceability.
- **Refresh token hashing** — only SHA-256 hashes are persisted; plaintext tokens are never stored.
- **Refresh token reuse detection** — presenting a revoked token revokes all of the user's active tokens.
- **Permission-based authorization** via a dynamic policy provider.
- **Security headers middleware** — `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, `Content-Security-Policy`, and more.
- **Magic-number upload validation** — files are verified by binary signature, restricted to PDF, PNG, JPEG, DOCX, and XLSX.
- **Rate limiting** — per-IP fixed-window policies for login, refresh, public, and protected endpoints.
- **Global exception handling** — sanitized, consistent error responses.

See [SECURITY.md](SECURITY.md) for the complete security overview.

---

## Performance Improvements

- Rate limiting partitioned per client IP to protect authentication and API endpoints.
- Database-level pagination for collection queries.
- Fully asynchronous EF Core data access.

---

## Authentication

- Login, logout, and profile endpoints.
- Refresh token endpoint with rotation.
- Change password and update profile flows.
- Multi-device sessions with per-token revocation.

---

## Authorization

- Role-Based Access Control linking Users → Roles → Permissions → Modules.
- Dynamic permission policies resolved at request time.
- Endpoint-level permission enforcement (for example, `[HasPermission("products", "view")]`).
- Management features for Users, Roles, Permissions, and Modules.

---

## Developer Experience

- Standardized API response envelope (`success`, `message`, `data`, `errors`).
- Generic pagination with search and sorting.
- FluentValidation wired through a MediatR validation behavior.
- Mapster for object mapping.
- OpenAPI document generation and a Scalar API documentation UI.
- Automatic migrations and seeding on startup (roles, modules, permissions, admin account).
- Docker Compose setup for API, SQL Server, and Seq.
- Structured logging with Serilog and correlation IDs.

See [Setup.md](Setup.md) to get running locally or with Docker.

---

## Testing

- Unit tests for the Application layer (validators, commands, queries).
- Unit tests for the Domain layer.
- Integration tests covering authentication, protected endpoints, and health checks.

```bash
dotnet test
```

---

## Known Limitations

- **Forgot/Reset Password** is not yet implemented (planned for v1.1).
- **Distributed caching** (Redis) and **in-memory caching** are not included (planned for v1.2).
- **Message brokers** (RabbitMQ/Kafka), **Outbox pattern**, and **multi-tenancy** are not included (planned for v2.0).
- File storage uses a local storage provider; cloud storage backends are not included.
- Email delivery uses SMTP; queued/asynchronous email delivery is planned.

---

## Future Roadmap

Planned work includes account recovery and email templates (v1.1), caching and background workers (v1.2), and a distributed, multi-tenant architecture (v2.0). See [ROADMAP.md](ROADMAP.md) for details.

---

## Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for the complete list of changes in this release.
