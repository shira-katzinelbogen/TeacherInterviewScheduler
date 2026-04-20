# C# Rules (TeacherInterviewScheduler)

## MANDATORY: Modification Protocol & Scope Control
- NO AUTOMATIC EDITS: You are strictly forbidden from modifying, creating, or deleting any files without receiving explicit, written permission for EACH specific file in the current chat session.
- READ-ONLY CONTEXT: Treat the entire codebase as READ-ONLY by default. You may read any file to understand context, but you must never write to it unsolicited.
- PRE-MODIFICATION PLAN: Before making any changes to existing files, you MUST:
  1. List every file you intend to modify.
  2. Briefly explain WHY the change is necessary for the current task.
  3. Wait for the user to say "Proceed" or "Approved" before applying any edits or using the "Apply" feature.
- NO "CLEANUP" EDITS: Do not perform "refactoring", "formatting", or "cleanup" in existing files that are not directly related to the specific task at hand.

---

## Architectural Alignment (Mandatory)
- Respect the project layered architecture:
  - ASP.NET Core Web API (Controllers)
  - Service Layer (BLL)
  - EF Core Repositories (Data Access)
- Logic must stay in Services: Domain validation, orchestration, and business rules belong in the Service layer.
- Data Access must stay in Repositories: EF Core queries/updates, relationship loading, and DbContext usage belong in Repositories.
- Controllers should be thin: Validate incoming request/DTO nulls and delegate business logic to Services.

## Documentation & Language
- Every new or updated method/class must include standard XML documentation: /// <summary>.
- All comments and technical documentation must be exclusively in English.
- No emojis in any output (code comments, logs, or chat text).

## General C# style
- Use file-scoped namespaces: namespace SchedulingService.Foo;
- Prefer sealed for:
  - Controllers
  - AutoMapper Profile implementations
- Use constructor injection for dependencies and store them in private readonly fields.
- Async method names must end with Async and return Task or Task<T>.

## Nullability (Strict)
- Treat nullable reference types as real (<Nullable>enable</Nullable> is on).
- Avoid the null-forgiving operator (!) unless you have already proven the value is non-null.
- Controllers: If a DTO parameter (from [FromBody]) is null, return BadRequest().
- Services: Validate inputs early using ArgumentNullException, ArgumentOutOfRangeException, or KeyNotFoundException.

## Async + EF Core usage
- Always use await for asynchronous work; avoid .Result / .Wait().
- For EF Core queries/updates, use async variants (ToListAsync, FirstOrDefaultAsync, etc.).
- Persist changes in the Service Layer by calling _db.SaveChangesAsync() after repository operations.

## Domain Invariants: Availability Overlap
- Use the exclusive range-end approach: [rangeStart, rangeEndExclusive].
- Two intervals overlap if:
  - left.StartTime < rightRangeEndExclusive && left.EndTime > rightRangeStart
- For "day" queries:
  - dayStart = date.Date
  - dayEndExclusive = dayStart.AddDays(1)

---

## Final Validation Step (Before Any Solution)
- Before providing or implementing any solution, cross-check it against:
  1. The Modification Protocol (Did I ask for permission before modifying?)
  2. Architectural Alignment.
  3. Strict Consistency.
  4. Documentation & Language rules.
  5. Domain logic, especially Availability Overlap semantics.