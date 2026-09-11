# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.2.0] - 2026-08-06

Health Checks dashboard and operational observability. Contains breaking changes
to the health endpoint — see the [release notes](maintenance/RELEASE_NOTES_v1.2.0.md).

### Added

- Health Checks UI dashboard at `/healthcheck-ui` (visual, with polling and history stored in SQL Server).
- Service-level health checks with friendly names and tags: Background Job Service: Hangfire (`Background Job`), Email Service: SMTP (`Email`, custom TCP check), and File Storage Service (`File Storage`, custom read/write probe); databases named and tagged `Database`.
- `/healthcheck-json` endpoint in UI format for Docker/Kubernetes probes and monitoring.
- HTTP Basic Auth protection for the Hangfire and Health Checks UI dashboards, validated against ASP.NET Identity (SuperAdmin role).
- `HealthChecksUI:HealthCheckEndpoint` configuration setting.

### Changed

- Health-check failure tiering: databases and file storage report `Unhealthy` (critical → `503`); Hangfire and SMTP report `Degraded` (still `200`).

### Removed

- `/health` raw JSON endpoint and its controller (replaced by `/healthcheck-json` and `/healthcheck-ui`).

### Security

- Operational dashboards (`/hangfire`, `/healthcheck-ui`) now require SuperAdmin authentication in all environments (Hangfire was previously open in Development).
- Pinned `KubernetesClient` 18.0.13 and `IdentityModel` 5.2.0 to resolve transitive dependencies/advisories introduced by the Health Checks UI package (no known vulnerabilities remain).

---

## [1.1.0] - 2026-08-06

Refinement and security hardening of the **File Attachments** feature. This
release contains breaking changes to the file-attachment endpoints — see the
[release notes](RELEASE_NOTES_v1.1.0.md) for the upgrade guide.

### Added

- Owner-module (resource-based) authorization for file attachments: access is derived from the permission of the module that owns the file, evaluated through the existing dynamic permission policy.
- `NotFoundException` and `ForbiddenException` types, mapped to `404` and `403` by the global exception middleware (available application-wide).
- Cascade cleanup of file attachments (`IFileAttachmentCleaner`): deleting a `Product` removes its attachments — database rows and physical files — preventing orphans.
- Required owner filter (`module`, `recordId`) on the file-attachment listing endpoint.

### Changed

- Unified file upload into a single endpoint that accepts one or more files (`POST /api/file-attachments`); the upload response is now always an array.
- File-attachment authorization now follows the owning module (for example, `products:create` to upload, `products:view` to read/list/download, `products:delete` to delete) instead of a flat `file-attachments:*` permission.

### Removed

- `POST /api/file-attachments/multiple` (folded into `POST /api/file-attachments`).
- `GET /api/file-attachments/module/{module}/{recordId}` (replaced by `GET /api/file-attachments?module=&recordId=`).
- Unfiltered listing behavior of `GET /api/file-attachments` (owner filter is now required).
- The `file-attachments` permission module from database seeding (no longer used).

### Fixed

- File-not-found now returns `404 Not Found` instead of `500` (download) or `200` with `success: false` (read/delete).

### Security

- Closed a cross-module data-exposure gap: attachment read/list/download/delete is now gated by the owning module's permission, so access to one module no longer grants access to another module's files through the generic endpoint.

---

## [1.0.0] - 2026-07-15

First stable release of the SearchEngine Backend API starter template.

### Added

- Clean Architecture solution structure (Domain, Application, Infrastructure, Presentation).
- CQRS implementation using MediatR with commands and queries organized by feature.
- JWT authentication with login, logout, and profile endpoints.
- Refresh token support with rotation and multi-device sessions.
- Change password functionality.
- Update profile functionality.
- Role-Based Access Control (RBAC) with users, roles, permissions, and modules.
- Dynamic permission policies with a permission authorization handler.
- Feature modules: Authentication, Users, Roles, Permissions, Modules, Products, Audit Logs, and File Attachments.
- File attachment upload (single and multiple), download, listing, and deletion.
- Audit logging via an EF Core `AuditableEntityInterceptor`.
- Standardized API response envelope (`success`, `message`, `data`, `errors`).
- Generic pagination with search and sorting support.
- FluentValidation validators wired through a MediatR validation pipeline behavior.
- Object mapping with Mapster.
- Health checks endpoint (`/health`) including SQL Server connectivity.
- OpenAPI document generation and Scalar API documentation UI.
- Automatic database migration and seeding on startup (default roles, modules, permissions, and administrator account).
- Docker Compose setup with API, SQL Server, and Seq services.

### Changed

- `Program.cs` kept minimal by delegating configuration to dedicated startup extension methods.
- Renamed the "Current User" endpoint to "Profile".
- Removed the password field from the update profile flow; password changes are handled by the dedicated change-password endpoint.

### Fixed

- Hardened refresh token storage so tokens are never persisted in plaintext (migration `HardenRefreshTokenStorage`).

### Security

- JWT bearer authentication.
- Refresh token rotation on every use.
- Refresh tokens hashed with SHA-256 before persistence; plaintext tokens are never stored.
- Refresh token reuse detection that revokes all active tokens for the affected user.
- Password hashing via ASP.NET Core Identity.
- Permission-based authorization with a dynamic policy provider.
- Security response headers middleware (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, `Content-Security-Policy`, and others).
- Magic-number (binary signature) validation for uploaded files.
- Rate limiting policies for login, refresh, public, and protected API endpoints.
- CORS policy configuration.
- Global exception handling middleware.
- Startup configuration validation for required settings.

### Performance

- Fixed-window rate limiting partitioned per client IP address.
- Asynchronous EF Core data access throughout the application.
- Pagination applied at the database query level.

### Architecture

- Strict Clean Architecture dependency direction (dependencies point inward).
- Result pattern (`Result<T>`) for consistent, strongly typed responses.
- Options pattern for typed configuration.
- Extension-based startup composition.
- Feature-oriented folder structure in the Application layer.
- Separate Identity, Persistence, and Shared infrastructure projects.
- Correlation ID, global exception, security headers, and permission middleware pipeline.

### Testing

- Unit tests for the Application layer (validators, commands, queries).
- Unit tests for the Domain layer.
- Integration tests covering authentication, protected endpoints, and health checks.

### Observability

- Structured logging with Serilog (Console, rolling file, and Seq sinks).
- Request logging and correlation IDs.
- OpenTelemetry instrumentation.
- Hangfire background processing backed by SQL Server storage.

[1.2.0]: https://github.com/adityapratamaf/SearchEngineProjectAPI/releases/tag/v1.2.0
[1.1.0]: https://github.com/adityapratamaf/SearchEngineProjectAPI/releases/tag/v1.1.0
[1.0.0]: https://github.com/adityapratamaf/SearchEngineProjectAPI/releases/tag/v1.0.0
