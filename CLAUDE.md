# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

A release and task management tool for PM, Developer, and QA teams. Full-stack: ASP.NET Core 10 API (`/server`) + React 18 SPA (`/client`).

---

## Dev Commands

### Server (run from `/server`)
```bash
dotnet build
dotnet run                         # runs on http://localhost:5105
dotnet watch run                   # hot reload
dotnet ef migrations add <Name>    # add a new migration
dotnet ef database update          # apply migrations manually
```
The server auto-migrates and seeds the database on startup via `DbSeeder.Seed()`. Database is SQLite (`projectmanagement.db`).

### Client (run from `/client`)
```bash
npm install
npm run dev      # http://localhost:5173, proxies /api → http://localhost:5105
npm run build    # runs tsc -b then vite build
```

---

## Architecture

### Server Layer Flow
```
Controller → Service → Repository → AppDbContext (EF Core / SQLite)
```

- **Controllers** — thin, HTTP only; `[Authorize(Roles = "...")]` applied here
- **Services** — all business logic and role enforcement (also enforce here, not just controller)
- **Repositories** — data access only, no business rules
- **DTOs** — always use; never expose entities directly
- **Validators** — FluentValidation; kept in `/Validators/`, one per DTO

### Key Directories
| Directory | Purpose |
|---|---|
| `Auth/` | `JwtTokenService` — generates access tokens (15 min) and refresh tokens (7 days, stored as hash) |
| `Data/` | `AppDbContext`, `DbSeeder` |
| `Configurations/` | EF Core Fluent API configs — `IEntityTypeConfiguration<T>`, NOT data annotations |
| `Exceptions/` | `AppException` base + `NotFoundException`, `ForbiddenException`, `BadRequestException`, `ConflictException` |
| `Middleware/` | `ExceptionHandlingMiddleware` — catches all `AppException` subclasses, returns `{ success, statusCode, message }` |
| `Enums/` | `UserRole` (PM, Developer, QA), `TaskStatus`, `ReleaseStatus` |

### Authentication Flow
- Login → returns `{ accessToken, refreshToken }`
- Access tokens are short-lived JWTs (15 min); refresh tokens are hashed in `RefreshToken` entity
- Revoked access tokens tracked by JTI in `RevokedToken` entity (blacklist checked in `OnTokenValidated`)
- Refresh token rotation: each refresh call revokes the old refresh token and issues a new pair
- Logout revokes both tokens

### Task Status Flow (strict, no skipping)
```
Pending → InProgress → PRRaised → Merged → Deployed → Done → QAApproved
```
Transition validation lives in `TaskStatusTransitionService`. The current enum has `QAApproved = 6` as an additional state beyond `Done`.

### Client Structure
- `src/api/axios.ts` — Axios instance with request interceptor (attaches Bearer token) and response interceptor (silent 401 refresh with request queue to prevent concurrent refreshes)
- `src/contexts/AuthContext.tsx` — auth state; tokens stored in `localStorage` under key `auth_data`
- `src/types/index.ts` — all shared TypeScript types/enums (keep in sync with server enums)
- Path alias `@` maps to `./src`

---

## Key Constraints

- **EF Core config**: Use Fluent API in `IEntityTypeConfiguration<T>` classes, never data annotations for DB mapping
- **Role enforcement**: Apply at controller level AND re-enforce in service layer
- **Error responses**: Always throw a typed `AppException` subclass — middleware handles formatting
- **Validation**: FluentValidation validators registered via `AddValidatorsFromAssemblyContaining<Program>()` and auto-wired via `AddFluentValidationAutoValidation()`
- **DI**: All services and repositories registered as `Scoped` in `Program.cs`
- **No tests** currently exist in the project
