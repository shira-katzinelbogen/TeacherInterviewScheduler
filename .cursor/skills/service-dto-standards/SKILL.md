---
name: service-dto-standards
description: Enforces C# service-layer persistence and DTO/mapping separation of concerns in the TeacherInterviewScheduler repository. Use when the user asks for help implementing or reviewing BLL service methods (Create/Update/Delete), DTOs, or mapping profiles (AutoMapper/Mapper), especially around SaveChanges/transactions and banning manual ToEntity/FromEntity conversions in DTO/Entity types.
globs: **/*.cs
alwaysApply: true
---

# Service & DTO Standards

## When to use
Use this when implementing or reviewing code in:
- `BLL/Services/*` (service-layer business logic)
- `BLL/Mapping/*` (Entity/DTO mapping)
- DTO types used by the API/service layer (request/response contracts)

## Mandatory rules
1. **Data Consistency (writes must persist)**
   - Every Service method that modifies the database must explicitly ensure persistence.
   - Ensure `SaveChanges()` / `SaveChangesAsync()` is called at the end of the operation (typically from the service layer).

2. **Transaction Management (multi-entity operations)**
   - For complex operations involving multiple entities/repositories where partial updates would break integrity, wrap the operation in a transaction.
   - Commit on success; rely on rollback via disposal/exception behavior.

3. **DTOs are pure data contracts**
   - DTO classes must be lightweight and contain only the data required for the request/response.
   - Do **not** include manual conversion logic inside DTOs (e.g., `ToEntity()`, `FromEntity()`).
   - Do **not** place such conversion logic inside Entities either.

4. **Mapping belongs to the dedicated layer**
   - All transformations between Entities and DTOs must be handled exclusively by the mapping layer (e.g., AutoMapper `Profile`s or dedicated mapper classes).
   - Service code should orchestrate and call the mapper rather than manually mapping fields.

## Quick checklist (agent verification)
- [ ] Is the Service method a write (Create/Update/Delete)?
- [ ] Does the method explicitly call `SaveChangesAsync()` / equivalent at the end?
- [ ] Does the operation touch multiple entities/repositories? If yes, is it in a transaction?
- [ ] Do DTOs contain only data (no `ToEntity/FromEntity` or manual mapping logic)?
- [ ] Are all Entity<->DTO transformations done in `BLL/Mapping/*` (or equivalent mapper)?

## Notes for applying changes
- In this repo’s conventions, persistence belongs in the service layer; repositories should mutate EF Core state, while the service performs the final `SaveChangesAsync()` call.
- If AutoMapper/mapper isn’t used for a transformation, add or extend the appropriate mapping profile instead of adding manual conversion code.

