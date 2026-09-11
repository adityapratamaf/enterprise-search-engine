# Permission System

A dynamic, relational **RBAC** (Role-Based Access Control) permission system.
Adding a new action or permission is **data-only** — no new columns, migrations,
DTO changes, or boolean flags are required.

---

## 1. Architecture

```
Modules ──1───∞── Permissions ──∞───1── PermissionActions
                       │
                       1
                       │
                       ∞
                 RolePermissions ──∞──(RoleId)── AspNetRoles
```

| Table | Role |
| --- | --- |
| `Modules` | Functional areas (e.g. `users`, `products`). |
| `PermissionActions` | Actions catalog (e.g. `View`, `Create`, `Export`). |
| `Permissions` | **Master** permission = one valid `Module × Action` (e.g. `users.view`). |
| `RolePermissions` | Grant = links a **Role** to a master **Permission**. |

Key properties:

- A **Permission** is the atomic unit of authorization. It has a stable `Code`
  (`"{module}.{action}"`, e.g. `users.view`) and an `IsActive` flag.
- **RolePermissions** references `RoleId` (FK to `AspNetRoles`) — not the role
  name — so renaming a role never breaks grants.
- No boolean flags anywhere. New actions (Print, Export, …) are just rows.

All permission tables live in the **Identity** database
(`ApplicationIdentityDbContext`).

---

## 2. Entities (ERD detail)

`Module` (`src/Core/SearchEngine.Domain/Entities/Module.cs`)

```
Id        (Guid, PK)
ModuleId  (string, unique)   -- business code, e.g. "users"
ModuleName(string)           -- display name
ModulePath(string)           -- frontend menu path, e.g. "/users"
+ audit fields
```

`PermissionAction` (`.../Entities/PermissionAction.cs`)

```
Id          (Guid, PK)
Code        (string, unique) -- e.g. "View", "Create"
Name        (string)
Description (string?)
DisplayOrder(int)
IsActive    (bool)
+ audit fields
```

`Permission` (`.../Entities/Permission.cs`) — **master catalog**

```
Id                 (Guid, PK)
ModuleId           (Guid, FK -> Modules.Id,          ON DELETE CASCADE)
PermissionActionId (Guid, FK -> PermissionActions.Id, ON DELETE RESTRICT)
Code               (string, unique)   -- "{module}.{action}", e.g. "users.view"
Description        (string?)
IsActive           (bool)
UNIQUE (ModuleId, PermissionActionId)
+ audit fields
```

`RolePermission` (`.../Entities/RolePermission.cs`) — **grant**

```
Id           (Guid, PK)
RoleId       (string, FK -> AspNetRoles.Id, ON DELETE RESTRICT)
PermissionId (Guid,   FK -> Permissions.Id, ON DELETE CASCADE)
UNIQUE (RoleId, PermissionId)
+ audit fields
```

> `PermissionAction -> Permission` is `RESTRICT` (not cascade) to avoid multiple
> cascade paths into `RolePermissions` on SQL Server.

---

## 3. How authorization works

Controllers declare required permission with the existing attribute:

```csharp
[HasPermission("users", "view")]      // module code, action code (case-insensitive)
public async Task<IActionResult> GetAll(...) { ... }
```

Flow:

1. `HasPermissionAttribute` builds the policy name `"users.view"`.
2. `PermissionPolicyProvider` turns it into a `PermissionRequirement(module, action)`.
3. `PermissionHandler` runs one efficient query (SQL `EXISTS`, `AsNoTracking`,
   nothing materialized):

```
RolePermissions
  JOIN AspNetRoles ON RolePermissions.RoleId = AspNetRoles.Id
  WHERE role.Name IN (user role claims)
    AND Permission.IsActive
    AND Permission.Module.Code        = module   (case-insensitive)
    AND Permission.PermissionAction.Code = action (case-insensitive)
```

If a matching grant exists, the requirement succeeds; otherwise access is denied
(default-deny).

### Action vocabulary

`View`, `Create`, `Update`, `Delete`, `Approve`, `Download`, `Export`, `Import`,
`Print`, `Assign`, `Execute`, `Archive`, `Restore`.

Controllers use the lowercase code, e.g. `[HasPermission("products", "create")]`.

---

## 4. Seeding

Run automatically on startup (`DatabaseExtensions.InitializeDatabasesAsync`) in
this order:

1. `DefaultRolesSeeder` — roles (`SuperAdmin`, `Admin`, `Member`) + admin user.
2. `ModuleSeeder` — modules.
3. `PermissionActionSeeder` — the 13 actions.
4. `PermissionCatalogSeeder` — master `Permissions` for every `Module × Action`.
5. `RolePermissionSeeder` — grants **all** permissions to `SuperAdmin`.

All seeders are **idempotent** (safe to run repeatedly).

---

## 5. How to add a new Module

Edit `src/Infrastructure/SearchEngine.Infrastructure.Identity/Seed/ModuleSeeder.cs`:

```csharp
new()
{
    Id = Guid.NewGuid(),
    ModuleId = "reports",     // business code
    ModuleName = "Reports",
    ModulePath = "/reports"
}
```

Creating a module through `POST /api/modules` (or the seeder) **immediately
generates** every `reports.*` permission via `PermissionCatalogSynchronizer`
(Permission is a **generated catalog** — never managed manually). `SuperAdmin`
is granted the new permissions on the next application startup
(`RolePermissionSeeder`). No migration is required.

---

## 6. How to add a new Permission Action

`PermissionAction` is a **managed master entity** with full CRUD (see §11), plus
`PermissionActionSeeder`, which guarantees the 13 default actions always exist
(idempotent). Adding an action — via API or seeder — is **data-only**: no schema
change, no migration.

When a new **active** action is created, `PermissionCatalogSynchronizer`
immediately generates `{module}.{action}` for every module (keeping the catalog
= `Modules × active PermissionActions`).

---

## 7. Assign / update permissions for a role

Both `POST` and `PUT` perform a **synchronization (diff)** — they do not
delete-and-recreate everything. Only removed actions are deleted and only new
actions are inserted, all inside a single atomic `SaveChanges` (one transaction).

```http
PUT /api/roles/{roleName}/permissions
Content-Type: application/json

{
  "moduleId": "7b0c...guid-of-module",
  "actionIds": [
    "aaaa...guid-of-View",
    "bbbb...guid-of-Create"
  ]
}
```

- `moduleId` = `Module.Id` (Guid).
- `actionIds` = the desired set of `PermissionAction.Id` for that module.
- Result: the role ends up with exactly those actions for the module.

Remove all permissions of a module for a role:

```http
DELETE /api/roles/{roleName}/permissions/{moduleId}
```

---

## 8. Read a role's permission matrix

```http
GET /api/roles/{roleName}/permissions
```

Response (catalog-driven — every action listed with a `granted` flag):

```json
{
  "success": true,
  "data": [
    {
      "moduleId": "7b0c...",
      "moduleName": "Users",
      "permissions": [
        { "actionId": "aaaa...", "actionName": "View",   "granted": true  },
        { "actionId": "bbbb...", "actionName": "Create", "granted": false }
      ]
    }
  ]
}
```

---

## 9. Current user permissions

`GET /api/auth/profile` (and the login response) return the user's **effective**
permissions, derived from the new model via `PermissionService`:

```json
{
  "menus": [
    { "moduleId": "users", "moduleName": "Users", "modulePath": "/users" }
  ],
  "permissions": [
    { "moduleId": "users", "moduleName": "Users", "actions": ["create", "view"] }
  ]
}
```

- `menus` = modules where the user has the `View` action.
- `permissions[].actions` = the action codes the user is granted for that module.

Frontend permission checks become, e.g., `actions.includes("create")`.

---

## 10. Permission is a generated catalog (no CRUD)

`Permission` is **never managed manually** — there is no Permission CRUD. It is
always the generated cross-product `Modules × active PermissionActions`, produced
idempotently by `PermissionCatalogSynchronizer`, which runs on:

- application startup (seeder),
- module creation (`CreateModule`),
- permission-action creation/activation (`CreatePermissionAction` / `UpdatePermissionAction`).

Deleting a module cascades to its permissions and grants. Retiring an action is
done with `IsActive = false` (non-destructive — see §11).

---

## 11. PermissionAction Management (CRUD)

Master-data management for the action catalog, gated by the dynamic
`permission-actions` module.

### Endpoints

| Method | Route | Permission |
| --- | --- | --- |
| GET | `/api/permission-actions` | `permission-actions.view` |
| GET | `/api/permission-actions/{id}` | `permission-actions.view` |
| POST | `/api/permission-actions` | `permission-actions.create` |
| PUT | `/api/permission-actions/{id}` | `permission-actions.update` |
| DELETE | `/api/permission-actions/{id}` | `permission-actions.delete` |

`PermissionActionResponse`: `{ id, code, name, description, displayOrder, isActive }`.

Create body: `{ code, name, description?, displayOrder, isActive }`.
Update body: `{ name, description?, displayOrder, isActive }` — **no `code`**.

### Validation (FluentValidation)

- `Code` required, unique.
- `Name` required, unique (case-insensitive).
- `DisplayOrder >= 0`.

### Business rules

1. **`Code` is immutable.** It is the authorization identifier used by
   `[HasPermission(module, action)]` and the basis of `Permission.Code`
   (`{module}.{action}`). Changing it would break authorization and orphan the
   catalog, so `Update` cannot change `Code` (only `Name`, `Description`,
   `DisplayOrder`, `IsActive`).
2. **Create auto-generates the catalog.** Creating an **active** action
   immediately generates `{module}.{action}` for every module. From that point
   the action **is part of the catalog**.
3. **Delete is guarded (no cascade).** A `PermissionAction` can be **hard-deleted
   only if it has never produced any `Permission`** (e.g. created inactive, or
   created then cancelled before use). If it is already part of the catalog,
   delete returns a business error — **retire it with `IsActive = false`
   instead**.
4. **Deactivation is non-destructive.** Setting `IsActive = false` removes the
   action from new catalog generation and assignment, but **existing permissions
   and role grants keep working** (runtime authorization checks `Permission.IsActive`,
   not `PermissionAction.IsActive`). No orphans, no mass revocation.
5. **Activation re-syncs the catalog.** Setting an action back to
   `IsActive = true` regenerates its `{module}.{action}` permissions.

### How to add a PermissionAction

```http
POST /api/permission-actions
Content-Type: application/json

{ "code": "Publish", "name": "Publish", "displayOrder": 14, "isActive": true }
```

Effect: the catalog immediately gains `{module}.publish` for every module. Use
`[HasPermission("<module>", "publish")]` on endpoints. `SuperAdmin` receives the
new grants on the next startup (`RolePermissionSeeder`).