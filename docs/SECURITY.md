# Security Policy

This document describes the security features implemented in the SearchEngine Backend API starter template, the security considerations behind them, and the process for reporting vulnerabilities.

The features listed below reflect the **current implementation**. Planned security-related work is tracked in the [ROADMAP](ROADMAP.md).

---

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

Security fixes are applied to the latest stable minor version.

---

## Implemented Security Features

### Authentication

Authentication is built on **ASP.NET Core Identity**. Users authenticate with email and password; passwords are hashed by the Identity password hasher and are never stored in plaintext.

### Authorization

Authorization is enforced through **Role-Based Access Control (RBAC)** combined with fine-grained permissions. Access to protected endpoints is evaluated against the authenticated user's roles and permissions.

### JWT

Successful authentication issues a signed **JWT bearer token**. The token carries the user's identity, roles, and status claims and is validated on every protected request. JWT settings (secret, issuer, audience) are provided through typed configuration and validated at startup.

### Refresh Token Rotation

Each time a refresh token is used, it is **rotated**: the presented token is revoked and a new refresh token is issued. The revoked token records the replacement token and the requesting IP address, enabling traceability across a token chain.

### Refresh Token Hashing

Refresh tokens are **never persisted in plaintext**. Only a **SHA-256 hash** (lowercase hexadecimal) of the token is stored in the database. Lookups are performed by hashing the presented token and matching against the stored hash.

### Refresh Token Reuse Detection

If a refresh token that has **already been revoked** is presented again, the system treats it as a **reuse (replay) attempt** and revokes **all active refresh tokens** for that user, forcing re-authentication. The revocation records the timestamp and originating IP address.

### RBAC

The authorization model links **Users → Roles → Permissions → Modules**. Roles group permissions, and permissions are scoped to modules, allowing granular control over which actions a user may perform.

### Permission Authorization

Endpoints are protected with a permission attribute (for example, `[HasPermission("products", "view")]`). A **dynamic permission policy provider** and permission authorization handler resolve and evaluate these requirements at request time, so permissions do not need to be pre-registered as static policies.

### Security Headers

A dedicated middleware adds hardened response headers to every response:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `Permissions-Policy` restricting geolocation, camera, microphone, payment, and USB.
- `X-Permitted-Cross-Domain-Policies: none`
- `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` for API responses (documentation endpoints are excluded so the UI can render).

The middleware also removes `X-Powered-By` and `Server` headers to reduce information disclosure.

### Magic Number Upload Validation

Uploaded files are validated by their **binary signature (magic number)**, not just their file extension. Only PDF, PNG, JPEG, DOCX, and XLSX files are accepted, and a file whose binary header does not match its declared extension is rejected. This mitigates disguised or malicious file uploads.

### Rate Limiting

Fixed-window **rate limiting** is applied per client IP address across distinct policies:

- `login`
- `refresh`
- `public`
- `api` (protected endpoints)

Limits are configurable and validated at startup. Requests exceeding a limit receive an HTTP `429 Too Many Requests` response in the standard response envelope.

### Global Exception Handling

A global exception handling middleware ensures unhandled errors return a consistent, sanitized response envelope instead of leaking stack traces or internal details to clients. A correlation ID is attached to requests to aid diagnostics without exposing internals.

---

## OWASP Considerations

The template addresses several risks from the **OWASP Top 10**:

- **Broken Access Control** — RBAC with permission-based authorization enforced per endpoint.
- **Cryptographic Failures** — password hashing via Identity; refresh tokens stored only as SHA-256 hashes.
- **Injection** — parameterized data access through Entity Framework Core and validated inputs via FluentValidation.
- **Insecure Design** — refresh token rotation and reuse detection; least-privilege permission model.
- **Security Misconfiguration** — hardened security headers, startup configuration validation, and reduced information disclosure.
- **Identification and Authentication Failures** — JWT validation, rate-limited authentication endpoints, and session revocation.
- **Security Logging and Monitoring Failures** — structured logging (Serilog/Seq), request logging, correlation IDs, and audit logging.
- **Server-Side Request / Malicious Upload risks** — magic-number file validation restricting accepted file types.

This list describes the mitigations present in the template. Operators remain responsible for secure deployment, secret management, TLS termination, and environment hardening.

---

## Reporting a Vulnerability

If you discover a security vulnerability, please report it **privately**. Do **not** open a public issue, pull request, or discussion for security matters.

To report a vulnerability:

1. Use GitHub's [private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability) for this repository, **or**
2. Contact the repository maintainer directly through their GitHub profile: [@adityapratamaf](https://github.com/adityapratamaf).

Please include:

- A description of the vulnerability and its impact.
- Steps to reproduce or a proof of concept.
- Affected version(s) and any relevant configuration.

---

## Responsible Disclosure

We ask that you follow responsible disclosure practices:

- Give us a reasonable opportunity to investigate and address the issue before any public disclosure.
- Avoid accessing, modifying, or deleting data that does not belong to you.
- Do not run automated scanning or denial-of-service tests against shared or production environments.

We will acknowledge valid reports, keep you informed of remediation progress, and credit reporters who wish to be recognized once a fix is released.
