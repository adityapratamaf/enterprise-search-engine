# Contributing Guide

Thank you for your interest in contributing to the **SearchEngine Backend API** starter template. This document describes the conventions and workflow used in this repository so that contributions remain consistent, reviewable, and easy to maintain.

Please read this guide before opening a pull request.

---

## Project Overview

SearchEngine Backend API is an ASP.NET Core 10 starter template built on **Clean Architecture** and the **CQRS** pattern (MediatR). It provides authentication, authorization (RBAC with dynamic permissions), auditing, file attachments, background processing, and observability out of the box.

The solution is organized into four layers:

```text
src
├── Core
│   ├── SearchEngine.Domain          # Entities, enums, domain models
│   └── SearchEngine.Application      # CQRS use cases, validation, interfaces
├── Infrastructure
│   ├── SearchEngine.Infrastructure.Identity      # Identity, JWT, permissions
│   ├── SearchEngine.Infrastructure.Persistence   # EF Core, business data
│   └── SearchEngine.Infrastructure.Shared        # Email, map, file storage
└── Presentation
    └── SearchEngine.WebAPI           # Controllers, middleware, startup
```

See [Architecture.md](Architecture.md) for a detailed description of each layer.

---

## Getting Started

1. Fork the repository and clone your fork.
2. Follow [Setup.md](Setup.md) to configure and run the project locally.
3. Create a feature branch (see [Branch Naming Convention](#branch-naming-convention)).
4. Make your changes, keeping them focused and self-contained.
5. Run the build, tests, and formatter before pushing.
6. Open a pull request against `main`.

```bash
dotnet restore
dotnet build
dotnet test
dotnet format
```

---

## Branch Naming Convention

Create a new branch for each change. Use a short, descriptive name prefixed by the type of work:

```text
feat/<short-description>
fix/<short-description>
refactor/<short-description>
docs/<short-description>
test/<short-description>
chore/<short-description>
```

Examples:

```text
feat/forgot-password
fix/refresh-token-expiry
docs/update-setup-guide
```

Use lowercase and hyphens (`kebab-case`). Do not commit directly to `main`.

---

## Git Workflow

This project uses a simple feature-branch workflow:

1. Sync your local `main` with upstream.
2. Create a feature branch from `main`.
3. Commit your work in small, logical units.
4. Push the branch to your fork.
5. Open a pull request into `main`.
6. Address review feedback and keep the branch up to date with `main`.
7. A maintainer merges the pull request once it is approved and green.

Keep pull requests small and focused. Unrelated changes should be split into separate branches and pull requests.

---

## Pull Request Process

Before requesting a review, make sure your pull request:

- [ ] Builds successfully (`dotnet build`).
- [ ] Passes all tests (`dotnet test`).
- [ ] Is formatted (`dotnet format`).
- [ ] Follows the coding and CQRS conventions described below.
- [ ] Includes tests for new behavior where applicable.
- [ ] Updates documentation when behavior or configuration changes.
- [ ] Uses a clear title following the [Commit Message Convention](#commit-message-convention).

Provide a concise description of **what** changed and **why**. Link any related issues. A maintainer will review and may request changes before merging.

---

## Coding Conventions

- Target **.NET 10** and C# language conventions used throughout the solution.
- Follow **Clean Architecture** dependency rules: dependencies point inward. The `Domain` layer has no dependencies; `Application` depends only on `Domain`; `Infrastructure` and `Presentation` depend on inner layers.
- Keep `Program.cs` minimal — register services through the existing extension methods (`AddPersistence()`, `AddIdentityInfrastructure()`, `AddSearchEngineRateLimiter()`, etc.).
- Use the **Result pattern** (`Result<T>`) for application and service return values instead of throwing for expected failures.
- Use **FluentValidation** validators for request validation; they run automatically through the MediatR validation behavior.
- Use **Mapster** for object mapping.
- Prefer `async`/`await` for all I/O-bound operations.
- Run `dotnet format` before committing; match the existing style of the files you touch.

---

## Folder Structure Convention

Each layer follows a consistent internal structure. New code should be placed in the layer that owns the responsibility:

| Layer          | Contains                                                     |
| -------------- | ----------------------------------------------------------- |
| Domain         | Entities, enums, domain models, base entities               |
| Application    | Features (CQRS), common behaviors, interfaces, models       |
| Infrastructure | EF Core contexts, identity, external services, migrations   |
| Presentation   | Controllers, middleware, startup extensions, options        |

Features in the Application layer are organized **by feature**, not by technical type:

```text
Features/<FeatureName>
├── Commands
│   └── <CommandName>
├── Queries
│   └── <QueryName>
├── DTOs
├── Validators
└── Mappings
```

---

## CQRS Convention

This project uses MediatR for CQRS. Follow these conventions when adding use cases:

**Commands** change state (create, update, delete). **Queries** read state (get, search, paginate, filter).

Each command or query lives in its own folder under the feature and typically contains:

- A request record — `CreateProductCommand`, `GetProductByIdQuery`.
- A handler — `CreateProductCommandHandler` implementing `IRequestHandler<TRequest, TResponse>`.
- A validator (for commands and non-trivial queries) — `CreateProductCommandValidator`.

Guidelines:

- One command/query per folder; name files after the use case.
- Handlers return `Result<T>` and must not throw for expected failures.
- Keep handlers thin — orchestrate domain logic and infrastructure interfaces; do not embed cross-cutting concerns (validation, logging) that pipeline behaviors already handle.
- Read queries should support pagination, search, and sorting where a collection is returned.

---

## Commit Message Convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/). Each commit message must start with a type, optionally a scope, and a short imperative summary:

```text
<type>(<optional scope>): <short summary>
```

Supported types:

| Type       | Use for                                              |
| ---------- | ---------------------------------------------------- |
| `feat`     | A new feature                                        |
| `fix`      | A bug fix                                            |
| `refactor` | A code change that neither fixes a bug nor adds a feature |
| `docs`     | Documentation-only changes                           |
| `test`     | Adding or updating tests                             |
| `chore`    | Build process, tooling, or maintenance changes       |

Examples:

```text
feat: implement change password functionality
fix: correct refresh token reuse detection
refactor: extract rate limiter configuration into extension
docs: add setup guide for Docker environment
test: add integration tests for auth endpoints
chore: update NuGet package versions
```

Keep the summary in the imperative mood, under ~72 characters, and add a body when additional context is helpful.

---

Thank you for helping improve the project.
