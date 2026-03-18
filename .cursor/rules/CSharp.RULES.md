---
description: Core C# conventions for TeacherInterviewScheduler
globs: **/*.cs
alwaysApply: true
---

# C# Rules (TeacherInterviewScheduler)

These rules are tailored to this repository’s current conventions:

- ASP.NET Core Web API, minimal hosting (`Program.cs`)
- EF Core (SQL Server) repositories that compose queries
- AutoMapper profiles
- `net8.0` with nullable reference types enabled (`<Nullable>enable</Nullable>`)

## Scope Control
- Do not modify, create, or delete any files outside the immediate scope of the current task without explicit user approval.

## Architectural Alignment (Mandatory)
- Respect the project layered architecture:
  - ASP.NET Core Web API (Controllers)
  - Service Layer (BLL)
  - EF Core Repositories (Data Access)
- Logic must stay in Services:
  - Domain validation, orchestration, and business rules belong in the Service layer.
- Data Access must stay in Repositories:
  - EF Core queries/updates, relationship loading, and DbContext usage belong in Repositories.
- Controllers should be thin:
  - Validate incoming request/DTO nulls and route/body invariants.
  - Delegate business logic to Services.

## Documentation & Language
- Every new or updated method/class must include standard XML documentation:
  - Use `/// <summary>` (and related XML docs when appropriate).
- All comments and technical documentation must be exclusively in English.

## Clean Interface
- Do not use emojis in any output (code comments, logs, or chat text).

## General C# style

- Use file-scoped namespaces: `namespace SchedulingService.Foo;`
- Prefer `sealed` for:
  - Controllers
  - AutoMapper `Profile` implementations
- Use constructor injection for dependencies and store them in `private readonly` fields.
- Keep method names aligned with behavior:
  - Async methods end with `Async` and return `Task`/`Task<T>`.
  - Non-async methods do not use async naming.
- Prefer expression-bodied members for simple query helpers (when the result is short and readable).

## Nullability (strict)

- Treat nullable reference types as real:
  - If a value can be `null`, check it before use.
  - Avoid the null-forgiving operator (`!`) unless you have already proven the value is non-null (e.g., inside a guard or conditional).
- Controllers:
  - If a DTO parameter (from `[FromBody]`) can be `null`, return `BadRequest()` when it is `null`.
- Services:
  - Validate inputs early with:
    - `ArgumentNullException` for `null` reference arguments
    - `ArgumentOutOfRangeException` for invalid numeric ranges/ids
  - For “not found” situations, prefer `KeyNotFoundException`.

## Async + EF Core usage

- Always use `await` for asynchronous work; avoid `.Result` / `.Wait()`.
- For EF Core queries/updates, use async variants (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, etc.).
- Keep EF Core query code in repositories; keep orchestration and business validation in services.

## Controllers (ASP.NET Core)

- Controllers should be `sealed` and use file-scoped namespaces.
- Use proper endpoint return types:
  - `Task<ActionResult<T>>` when an endpoint can return either a success payload or an error.
  - `Task<IActionResult>` when there is no success payload.
- Route/body invariants:
  - When route and body identify the same entity, validate they match; otherwise return `BadRequest(...)`.
- Ownership/scoping safety:
  - For deletes/updates scoped by route (for example `studentId`), ensure the entity belongs to the scoped route id before deleting.
- For create endpoints:
  - Return `CreatedAtAction(...)` (or `CreatedAtRoute(...)`) after successful creation.

## Services (BLL)

- Services enforce domain rules and invariants. Repositories focus on data access.
- Follow the existing exception style:
  - `ArgumentNullException` / `ArgumentOutOfRangeException` / `ArgumentException` for invalid inputs/state
  - `KeyNotFoundException` for missing entities
  - `InvalidOperationException` for domain rule violations (e.g., overlap conflicts)
- Persist changes in the service layer:
  - Call `_db.SaveChangesAsync()` after repository operations (matching current patterns in this repo).
- Keep helper validation centralized (e.g., a `ValidateStartEnd(start, end)` method).

## Repositories (DAL/BLL Repos)

- Repositories should not implement business validation; they should compose queries and mutate the EF Core context.
- Read methods:
  - Return `IReadOnlyList<T>` or `T?` appropriately.
  - Don’t throw for “not found”; return `null`/empty results as appropriate.
- Write methods:
  - Prefer “context mutation only” semantics (caller/service saves changes).
- Keep EF Core overlap logic consistent with this repo’s semantics (see next section).

## Domain invariants: Availability overlap

Availability date/time overlap semantics in this repo must use the exclusive range-end approach:

- Treat a day/time window as `[rangeStart, rangeEndExclusive]`.
- Two intervals overlap if:
  - `left.StartTime < rightRangeEndExclusive && left.EndTime > rightRangeStart`

When implementing “day” queries:

- Use `dayStart = date.Date`
- Use `dayEndExclusive = dayStart.AddDays(1)`
- Query overlaps with the same predicate as above.

## Final Validation Step (Before Any Solution)
- Before providing or implementing any solution, cross-check it against:
  - Architectural Alignment
  - Strict Consistency
  - Documentation & Language rules
  - Domain logic, especially Availability Overlap semantics

