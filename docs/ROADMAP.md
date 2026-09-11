# Roadmap

This document outlines the planned evolution of the **SearchEngine Backend API Starter Template**.

It provides a high-level overview of completed milestones and future development plans.

> **Note**
> The roadmap is subject to change based on project priorities and community feedback.
> For detailed implementation history, see the [CHANGELOG](CHANGELOG.md).

---

# v1.0 — SearchEngine Foundation ✅

The initial release establishes the foundation for building searchengine-grade ASP.NET Core applications following **Clean Architecture** and **CQRS** principles.

### Architecture

- ✅ Clean Architecture
- ✅ CQRS with MediatR
- ✅ Repository-free architecture
- ✅ Dependency Injection
- ✅ Modular feature-based structure

### Authentication & Authorization

- ✅ ASP.NET Core Identity
- ✅ JWT Authentication
- ✅ Refresh Token
- ✅ Login
- ✅ Logout
- ✅ Current User
- ✅ Role Management
- ✅ Permission Management
- ✅ Authorization Policies

### Core Features

- ✅ User Management
- ✅ Product Module (Sample CRUD)
- ✅ Audit Logging

### Validation

- ✅ FluentValidation
- ✅ Validation Pipeline Behavior

### Mapping

- ✅ Mapster
- ✅ Feature-based Mapping Configuration

### API Features

- ✅ Global Exception Handler
- ✅ Generic API Response
- ✅ Generic Pagination
- ✅ Search
- ✅ Sorting

### Observability

- ✅ Serilog
- ✅ OpenTelemetry
- ✅ Correlation ID
- ✅ Health Checks

### Infrastructure

- ✅ SQL Server
- ✅ Entity Framework Core
- ✅ ASP.NET Identity
- ✅ File Upload
- ✅ Hangfire

### Security

- ✅ Rate Limiting
- ✅ JWT Authorization
- ✅ Refresh Token Rotation

### Documentation

- ✅ OpenAPI
- ✅ Scalar API Documentation

### Testing

- ✅ Unit Tests
- ✅ Integration Tests

---

# v1.1 — Identity & User Experience

Focus on completing the authentication lifecycle and improving user management.

### Authentication

- Forgot Password
- Reset Password
- Email Verification
- Change Password
- Change Email
- Revoke Refresh Token
- Session Management

### User Management

- User Registration
- User Profile
- Avatar Upload
- User Preferences

### Email

- Email Templates
- Email Notifications
- Notification Service

---

# v1.2 — Performance & Background Processing

Improve application performance and asynchronous processing.

### Caching

- IMemoryCache
- Redis Cache
- Cache Invalidation

### Background Processing

- Hangfire Dashboard Improvements
- Background Workers
- Email Queue
- Scheduled Jobs

### Performance

- Response Compression
- Output Cache Improvements
- Query Optimization

---

# v1.3 — File & Storage

Enhance document management capabilities.

### File Management

- Image Upload
- Document Upload
- File Versioning
- Virus Scan Support

### Storage

- Local Storage
- Azure Blob Storage
- AWS S3 Storage
- MinIO Support

---

# v1.4 — Observability & Monitoring

Production-ready monitoring and diagnostics.

### Logging

- Structured Logging Improvements
- Audit Enhancements

### Monitoring

- Prometheus Metrics
- Grafana Dashboard
- Application Metrics

### Tracing

- Jaeger
- Zipkin

---

# v2.0 — Distributed Architecture

Support searchengine-scale distributed systems.

### Messaging

- RabbitMQ
- Kafka
- Event Bus
- Outbox Pattern

### Architecture

- Domain Events
- Integration Events
- Event-Driven Architecture

### Multi-Tenant

- Tenant Resolution
- Tenant Isolation
- Per-Tenant Configuration

### Deployment

- Docker Compose
- Kubernetes
- Helm Charts

---

# v2.1 — Cloud Native

Cloud-first deployment support.

### Cloud Providers

- Microsoft Azure
- Amazon Web Services (AWS)
- Google Cloud Platform (GCP)

### DevOps

- GitHub Actions
- Azure DevOps Pipelines
- SonarQube
- Code Coverage Reports

---

# Long-Term Vision

The long-term goal is to provide a production-ready starter template that includes:

- SearchEngine Clean Architecture
- CQRS
- Authentication & Authorization
- Modular Feature Design
- Distributed Messaging
- Cloud-Native Deployment
- Built-in Observability
- SearchEngine Testing Strategy
- Production Best Practices

---

# Contributing

Suggestions, ideas, and feature requests are welcome.

Please open an issue or submit a pull request to help improve this project.

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.