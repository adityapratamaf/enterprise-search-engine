# SearchEngine Backend Architecture

SearchEngine Backend API is built using modern ASP.NET Core practices with a strong focus on maintainability, scalability, and separation of concerns.

---

# Architecture Principles

This project follows:

* Clean Architecture
* CQRS Pattern (MediatR)
* Domain-Driven Design (DDD) Principles
* Separation of Concerns
* Dependency Injection
* Result Pattern
* Generic Pagination
* Extension-Based Startup Configuration
* Options Pattern

---

# Layer Architecture

```text
                Presentation
                      │
                      ▼
                Application
                      │
                      ▼
                   Domain
                      ▲
                      │
              Infrastructure
```

### Responsibilities

| Layer          | Responsibility                           |
| -------------- | ---------------------------------------- |
| Presentation   | HTTP Pipeline, Middleware, API Endpoints |
| Application    | Business Use Cases, CQRS, Validation     |
| Domain         | Business Model and Rules                 |
| Infrastructure | Database, Identity, External Services    |

---

# Solution Structure

```text
src
├── Core
│   ├── SearchEngine.Domain
│   └── SearchEngine.Application
│
├── Infrastructure
│   ├── SearchEngine.Infrastructure.Identity
│   ├── SearchEngine.Infrastructure.Persistence
│   └── SearchEngine.Infrastructure.Shared
│
└── Presentation
    └── SearchEngine.WebAPI
```

---

# Domain Layer

Responsible for business entities and core business rules.

Contains:

* Entities
* Enums
* Domain Models
* Base Entities
* Domain Interfaces
* Common Classes

Example:

```text
SearchEngine.Domain
├── Common
├── Entities
└── Enums
```

---

# Application Layer

Contains all application use cases.

Responsibilities:

* Commands
* Queries
* DTOs
* Validators
* CQRS Handlers
* Behaviors
* Interfaces
* Mapping
* Pagination
* Result Pattern

Feature Structure

```text
Products
├── Commands
├── Queries
├── DTOs
├── Validators
└── Mappings
```

Every feature is organized independently to improve maintainability.

---

# Infrastructure Layer

Infrastructure is divided into multiple projects.

## SearchEngine.Infrastructure.Identity

Responsible for:

* ASP.NET Identity
* JWT Authentication
* Refresh Token Management
* Authorization Policies
* Permission Handler
* Identity Services
* Database Seeding
* Identity Database
* Identity Migrations

Structure

```text
Identity
├── Authorization
├── Context
├── Entities
├── Mappings
├── Migrations
├── Seed
├── Services
└── DependencyInjection.cs
```

---

## SearchEngine.Infrastructure.Persistence

Responsible for:

* Business Database
* Entity Framework Core
* Database Context
* Entity Interceptors
* Database Migrations

Structure

```text
Persistence
├── Context
├── Interceptors
├── Migrations
├── Services
└── DependencyInjection.cs
```

---

## SearchEngine.Infrastructure.Shared

Contains reusable infrastructure services.

Examples:

* Email Service
* File Storage
* Cache Provider
* External APIs
* Notification Services

---

# Presentation Layer

Responsible for exposing HTTP APIs.

Contains:

* Controllers
* Middleware
* Logging
* OpenAPI
* Scalar Documentation
* Health Checks
* Startup Extensions
* Configuration Options

Example

```text
SearchEngine.WebAPI
├── Controllers
├── Extensions
├── HealthChecks
├── Logging
├── Middleware
├── OpenApi
└── Options
```

---

# Startup Architecture

Program.cs remains minimal by delegating configuration into extension methods.

Example

```text
Program.cs
│
├── AddPersistence()
├── AddIdentityInfrastructure()
├── AddSearchEngineCors()
├── AddSearchEngineRateLimiter()
├── AddSearchEngineHangfire()
├── AddSearchEngineOpenTelemetry()
└── AddSearchEngineHealthChecks()
```

This keeps the startup pipeline clean and maintainable.

---

# CQRS Pattern

Commands

Used for:

* Create
* Update
* Delete

Queries

Used for:

* Read
* Search
* Pagination
* Filtering
* Sorting

---

# Result Pattern

All application responses use:

```csharp
Result<T>
```

Benefits

* Consistent API Response
* Centralized Error Handling
* Frontend Friendly
* Strongly Typed

---

# Pagination

Supports:

* Pagination
* Search
* Sorting

Example

```csharp
await query.ToPaginatedResultAsync<
    Product,
    ProductDto>(request);
```

---

# Authentication

Authentication is implemented using ASP.NET Identity.

Features:

* JWT Authentication
* Refresh Token Rotation
* Multi Device Login
* Session Tracking
* Logout
* Logout All Devices
* Token Revocation

---

# Authorization

Authorization is based on RBAC.

```text
User
    │
    ▼
Role
    │
    ▼
Permission
    │
    ▼
Module
```

Supports:

* Dynamic Permission Policies
* Permission-Based Authorization
* Module Permissions

---

# Middleware Pipeline

Global middleware includes:

* Correlation ID
* Global Exception Handling
* Permission Middleware

---

# Validation

Validation is implemented using:

```text
FluentValidation
        +
MediatR Pipeline Behavior
```

---

# Mapping

Object mapping uses:

```text
Mapster
```

instead of AutoMapper.

---

# Logging & Monitoring

Implemented using:

* Serilog
* Seq
* OpenTelemetry

Provides:

* Structured Logging
* Request Logging
* Distributed Tracing

---

# Background Processing

Background jobs are implemented using:

* Hangfire
* SQL Server Storage

---

# Health Monitoring

Application health is exposed through Health Checks.

Supports:

* SQL Server Connectivity
* Application Status

---

# Rate Limiting

Built using ASP.NET Core Rate Limiting.

Policies include:

* Login
* Refresh Token
* Public API
* Protected API

---

# Security

Implemented Features

* JWT Authentication
* Refresh Token Rotation
* RBAC
* Permission Authorization
* Audit Logging
* Multi Device Session
* Rate Limiting
* CORS

---

# Docker Support

Application can be executed using Docker Compose.

Included Services:

* API
* SQL Server
* Seq

---

# Testing

Supports:

* Unit Testing
* Integration Testing

---

# Future Improvements

Potential future enhancements:

* IMemoryCache
* Distributed Cache
* Redis
* Background Job Scheduling
* API Versioning
* Event Bus
* Outbox Pattern
* Multi-Tenant Architecture
* Kubernetes Deployment
* Distributed Tracing Exporter (Jaeger / OTLP)
