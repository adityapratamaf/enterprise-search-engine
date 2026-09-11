# Release Notes — v1.1.0

**Release date:** 2026-08-06

---

## Overview

SearchEngine Backend API **v1.1.0** refines the **File Attachments** feature toward the v1.0 release-candidate quality bar. It consolidates the endpoint surface, derives authorization from the **owning module** (closing a cross-module data-exposure gap), corrects HTTP status codes, and adds automatic cascade cleanup so deleting a record no longer leaves orphaned files.

This release contains **breaking API changes** to the file-attachment endpoints. See [Breaking Changes](#breaking-changes) and the [Upgrade Guide](#upgrade-guide) before updating clients.

> **Versioning note.** These changes are backward-incompatible for the file-attachment endpoints. For this starter template, a **minor** bump with a documented breaking-changes section is used (reserving `2.0.0` for the planned distributed-architecture milestone in the [roadmap](ROADMAP.md)). Treat the [Breaking Changes](#breaking-changes) section as required reading when upgrading.

---

## Highlights

- One upload endpoint for single **and** multiple files (`POST /api/file-attachments`).
- Listing is now scoped and safe: `GET /api/file-attachments?module=&recordId=` (owner filter required).
- Authorization follows the **owning module** (e.g., `products:view`), not a flat `file-attachments:*` permission.
- Correct HTTP semantics: `404 Not Found` and `403 Forbidden` where previously `500`/`200` were returned.
- Cascade cleanup: deleting a `Product` also removes its attachments (database rows **and** physical files).

---

## Breaking Changes

| Area | Before | After |
| ---- | ------ | ----- |
| Multiple upload | `POST /api/file-attachments/multiple` | **Removed.** Use `POST /api/file-attachments` (accepts one or many files). |
| Upload response | Single object for single upload | **Always an array** (`data: [ ... ]`), even for one file. |
| List by owner | `GET /api/file-attachments/module/{module}/{recordId}` | **Removed.** Use `GET /api/file-attachments?module=&recordId=`. |
| List all | `GET /api/file-attachments` returned all files | **`module` + `recordId` are now required** (`400` if missing). It no longer returns unfiltered results. |
| Authorization | Flat `file-attachments:create/view/download/delete` | **Owning-module permission** — `create` to upload, `view` to list/read/download, `delete` to delete (e.g., `products:*`). |
| Permission seed | `file-attachments` permission module was seeded | **Removed from seeding.** |

> These changes affect any client of the file-attachment endpoints and any role that relied on the `file-attachments` permission. Follow the [Upgrade Guide](#upgrade-guide).

---

## Endpoints (current surface)

| Method | Route | Purpose | Required permission |
| ------ | ----- | ------- | ------------------- |
| `POST` | `/api/file-attachments` | Upload one or more files (`multipart/form-data`: `Module`, `RecordId`, `Files`) | `{module}:create` |
| `GET` | `/api/file-attachments?module=&recordId=` | List a record's files (paginated) | `{module}:view` |
| `GET` | `/api/file-attachments/{id}` | File metadata | `{module}:view` |
| `GET` | `/api/file-attachments/{id}/download` | Download file content | `{module}:view` |
| `DELETE` | `/api/file-attachments/{id}` | Delete a file | `{module}:delete` |

`{module}` is the owning module of the file (for example, `products`), resolved from the request for upload/list and from the stored record for read/download/delete.

---

## Security & Correctness

- **Owner-module authorization (resource-based).** File access is now gated by the permission of the module that owns the file, evaluated at request time through the existing dynamic permission policy. A user with access to one module can no longer read or download another module's attachments through the generic endpoint.
- **Proper status codes.** A missing file now returns `404 Not Found` (previously `500` on download and `200` with `success: false` on read/delete). Unauthorized access returns `403 Forbidden`. Backed by new `NotFoundException`/`ForbiddenException` types mapped in the global exception middleware — available to the whole application, not only file attachments.
- **Cascade cleanup.** Because attachments use a polymorphic reference (no database foreign key), deleting an owning record previously left orphaned rows and files. Deleting a `Product` now removes its attachments (rows first, then physical files), preventing orphans.

---

## Upgrade Guide

**Clients / frontend**

1. Replace calls to `POST /api/file-attachments/multiple` with `POST /api/file-attachments` and send one or many files under the `Files` field.
2. Always read the upload response as an **array** (`data[0]`, `data[1]`, …), even for a single file.
3. Replace `GET /api/file-attachments/module/{module}/{recordId}` and any unfiltered `GET /api/file-attachments` with `GET /api/file-attachments?module=&recordId=`.
4. Ensure roles that manage attachments have the **owning module's** permissions (e.g., `products:create`, `products:view`, `products:delete`). Missing permission now returns `403`.

**Existing databases**

- The previously seeded `file-attachments` permission module and its generated permissions remain in existing databases but are **no longer used** (harmless). To remove them, either delete the `file-attachments` module via `DELETE /api/modules/{id}` (its generated permissions are removed with it) or re-seed from a clean database. Fresh databases no longer seed it.
- No schema migration is required; all changes are code- and seed-level.

---

## Notes & Known Limitations

- **Cascade cleanup is wired for `Product`** (Guid-keyed business entity). Wiring it for other delete flows is a one-line call to `IFileAttachmentCleaner`. Identity entities (User/Role) use string keys and are not wired yet, pending an owner-id design decision.
- **Pre-save upload (draft → link)** — uploading a file before its owning record exists — is **not** included. The supported flow is "save the record first, then upload" using the owning record's id as `RecordId`.
- **No update-file endpoint by design.** Files are immutable; replace = delete + upload. For a single-value image, store the file id on the owning record and swap the reference.

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

See [CHANGELOG.md](CHANGELOG.md) for the complete list of changes in this release.
