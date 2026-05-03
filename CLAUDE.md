# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

A release and task management tool for PM, Developer, and QA teams. Full-stack: ASP.NET Core 10 API (`/server`) + React 18 SPA (`/client`).

---

## Dev Commands

### Server (run from `/server`)
```bash
dotnet build ProjectManagement.slnx
dotnet run --project ProjectManagement.Presentation         # runs on http://localhost:5105
dotnet watch run --project ProjectManagement.Presentation   # hot reload
dotnet ef migrations add <Name> --project ProjectManagement.DAL --startup-project ProjectManagement.Presentation
dotnet ef database update --project ProjectManagement.DAL --startup-project ProjectManagement.Presentation
```
The server auto-migrates and seeds the database on startup via `DbSeeder.SeedAsync()`. Database is SQLite (`projectmanagement.db`).

### Client (run from `/client`)
```bash
npm install
npm run dev      # http://localhost:5173, proxies /api → http://localhost:5105
npm run build    # runs tsc -b then vite build
```

---

## Architecture

### N-Tier Project Structure
```
ProjectManagement.Presentation → ProjectManagement.BLL → ProjectManagement.DAL
ProjectManagement.Presentation → ProjectManagement.DAL   (for DI wiring only)
```

Three separate `.csproj` projects under `/server`, tied together by `ProjectManagement.slnx`:

| Project | Namespace | Purpose |
|---|---|---|
| `ProjectManagement.DAL` | `ProjectManagement.DAL` | Entities, DTOs, Enums, Exceptions, Repositories, EF Core, Migrations |
| `ProjectManagement.BLL` | `ProjectManagement.BLL` | Service interfaces & implementations, FluentValidation validators |
| `ProjectManagement.Presentation` | `ProjectManagement.Presentation` | Controllers, Middleware, Program.cs |

### Server Layer Flow
```
Controller (Presentation) → Service (BLL) → Repository (DAL) → AppDbContext (EF Core / SQLite)
```

- **Controllers** — thin, HTTP only; `[Authorize(Roles = "...")]` applied here; namespace `ProjectManagement.Presentation`
- **Services** — all business logic and role enforcement; namespace `ProjectManagement.BLL`
- **Repositories** — data access only, no business rules; namespace `ProjectManagement.DAL`
- **DTOs** — live in DAL (to avoid circular dependency with repository interfaces); namespace `ProjectManagement.DAL`
- **Validators** — FluentValidation; in `ProjectManagement.BLL/Validators/`; registered via `AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>()`

### Key Directories
| Path | Purpose |
|---|---|
| `ProjectManagement.DAL/Data/` | `AppDbContext`, `DbSeeder` |
| `ProjectManagement.DAL/Configurations/` | EF Core Fluent API configs — `IEntityTypeConfiguration<T>`, NOT data annotations |
| `ProjectManagement.DAL/Exceptions/` | `AppException` base + `NotFoundException`, `ForbiddenException`, `BadRequestException`, `ConflictException` |
| `ProjectManagement.DAL/Repositories/` | Repository interfaces and implementations |
| `ProjectManagement.DAL/DTOs/` | All request/response DTOs |
| `ProjectManagement.BLL/Services/` | Service interfaces and implementations |
| `ProjectManagement.BLL/Validators/` | FluentValidation validators |
| `ProjectManagement.Presentation/Controllers/` | API controllers |
| `ProjectManagement.Presentation/Middleware/` | `ExceptionHandlingMiddleware` — catches all `AppException` subclasses, returns `{ success, statusCode, message }` |

### DI Extension Methods
- `AddDAL(IConfiguration)` in `ProjectManagement.DAL/DependencyInjection.cs` — registers DbContext, Identity, Repositories
- `AddBLL()` in `ProjectManagement.BLL/DependencyInjection.cs` — registers FluentValidation, Services

### Namespace Disambiguation
The `ProjectManagement.DAL.Task` entity conflicts with `System.Threading.Tasks.Task`. Resolution:
- In any file that uses both: add `using Task = System.Threading.Tasks.Task;` and reference the entity as `ProjectManagement.DAL.Task`
- `TaskStatus` similarly: add `using TaskStatus = ProjectManagement.DAL.TaskStatus;`

### Authentication Flow
Auth uses **ASP.NET Core Identity with cookie sessions** (not JWT).

- `GET /api/auth/antiforgery` — issues `XSRF-TOKEN` cookie (readable by JS); must be called before any state-changing request
- `POST /api/auth/login` — `SignInManager.PasswordSignInAsync`; sets `auth_session` cookie (HttpOnly) and regenerates `XSRF-TOKEN`; returns `{ username, role }`
- `GET /api/auth/me` — returns current user from claims; used on page load to restore session
- `POST /api/auth/logout` — `SignInManager.SignOutAsync`; clears `auth_session` cookie
- All non-GET/HEAD/OPTIONS requests (except `/auth/login`) require a valid `X-XSRF-TOKEN` header

### Task Status Flow (strict, no skipping)
```
Pending → InProgress → PRRaised → Merged → Deployed → Done → QAApproved
```
Transition validation lives in `TaskStatusTransitionService`. The current enum has `QAApproved = 6` as an additional state beyond `Done`.

### Client Structure
- `src/api/axios.ts` — Axios instance with `withCredentials: true`; request interceptor reads `XSRF-TOKEN` cookie and attaches it as `X-XSRF-TOKEN` header; 401 response redirects to `/login`
- `src/contexts/AuthContext.tsx` — auth state held in-memory (no localStorage); on mount fetches antiforgery token then calls `/api/auth/me` to restore session
- `src/types/index.ts` — all shared TypeScript types/enums (keep in sync with server enums)
- Path alias `@` maps to `./src`

---

## Key Constraints

- **EF Core config**: Use Fluent API in `IEntityTypeConfiguration<T>` classes, never data annotations for DB mapping
- **Role enforcement**: Apply at controller level AND re-enforce in service layer
- **Error responses**: Always throw a typed `AppException` subclass — middleware handles formatting
- **Validation**: FluentValidation validators registered via `AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>()` in BLL's `AddBLL()` extension
- **DI**: All services and repositories registered as `Scoped` via `AddDAL()` / `AddBLL()` extension methods called from `Program.cs`
- **No tests** currently exist in the project
