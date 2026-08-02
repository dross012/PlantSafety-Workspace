# AccidentManagement Module Playbook

Phase 4 domain-modernization pass, third module. Status as of 2026-07-29.

## Domain model

Single aggregate root: `Accident` (`PlantSafety.Domain.AccidentManagement.Aggregates.Accident`), with a `CorrectiveAction` child entity collection (CAPA — Corrective and Preventive Action). Investigation workflow: Reported → SiteSecuredAndTriaged → InvestigatorAssigned → RootCauseRecorded → CorrectiveActionsInProgress → (RegulatoryReportFiled if required) → Closed → Archived.

Already a rich aggregate going into this pass, same maturity tier as PermitToWork/RiskManagement — and further ahead than either on tenant isolation (see below).

## Tenant isolation — already correct going in

Unlike RiskManagement's original state, `IAccidentRepository.GetByIdAsync` and `usp_Accident_Update`/`usp_Accident_GetById` already required and filtered on `TenantId` before this pass started (added by an earlier "Migration Commander" pass). No fix needed here — confirmed, not assumed, by reading the actual proc bodies. `ListAsync` (Domain write-side) still has no tenant parameter, same as RiskManagement's dormant `ListAsync` — left as-is since it has no callers either, consistent with the RiskManagement precedent.

## Commands

6 investigation-workflow commands (`AssignAccidentInvestigatorCommand`, `RecordAccidentRootCauseCommand`, `CreateAccidentCorrectiveActionCommand`, `CompleteAccidentCorrectiveActionCommand`, `FileAccidentRegulatoryReportCommand`, `CloseAccidentInvestigationCommand`) plus `CreateAccidentCommand`. All 6 mutating commands now implement `IConcurrencyAwareCommand` and carry an `ETag` (added this pass).

## Queries

**Closed this pass:** `GetAccidentByIdQuery` / `AccidentItemDto` / `GetAccidentByIdQueryHandler`, backed by `Application.AccidentManagement.Repositories.IAccidentRepository` (read-side, distinct namespace from the Domain write-side interface of the same name) / `AccidentReadDapperRepository`. Exposed as `GET /api/v1/accidentmanagement/{accidentId}`. Reuses the existing `usp_Accident_GetById` (already tenant-scoped and ETag-projecting) rather than adding a duplicate proc — only added canonical TRY/CATCH+ErrorLog to it, which it was missing. Exact mirror of RiskManagement's `GetRiskByIdQuery` fix.

## Events

Outbox-enqueue-on-update bug (found in PermitToWork, confirmed platform-wide across 54 files including this one) was already fixed in the earlier platform-wide sweep — `AccidentDapperRepository.UpdateAsync` already enqueues domain events correctly. No action needed this pass.

## Stored procedures / repository

`AccidentDapperRepository` under `PlantSafety.Infrastructure.AccidentManagement.Persistence`, backed by `usp_Accident_{Insert,Update,GetById,GetAllActive}` + `usp_CorrectiveAction_{Upsert,GetByAccidentId}` in the `[AccidentManagement]` schema.

**Closed this pass:** same "schema-ready but unenforced" ETag gap as RiskManagement had — `Accident.RowVersion` and the ETag projection already existed, but `usp_Accident_Update` took no `@ETag` and had no concurrency guard. Added the `@ETag NVARCHAR(64) = NULL` parameter, the `AND (@ETag IS NULL OR CONVERT(...) = @ETag)` WHERE guard (alongside the pre-existing TenantId condition), `@RowsAffected` capture before the audit INSERT, and `SELECT @RowsAffected` as the final statement (replacing the old ETag-only SELECT — clients now re-fetch the fresh ETag via `GetAccidentByIdQuery` instead). `IAccidentRepository.UpdateAsync` gained `string? eTag`; `AccidentDapperRepository.UpdateAsync` now calls it via `ExecuteScalarAsync` and throws `ConcurrencyConflictException` on zero rows affected — checked immediately after the parent `Accident` UPDATE and before the `CorrectiveAction` child-upsert loop, so a stale-ETag conflict aborts the whole transaction without touching child rows.

## API

`AccidentManagementV1Controller` — thin MediatR dispatcher, already versioned/policy-authorized. All 6 mutating request DTOs now carry `ETag`; the `close` endpoint previously took no request body at all, so a new `CloseAccidentInvestigationRequest` (ETag-only) was added.

**Benefits automatically from platform-wide fixes made during the PermitToWork/RiskManagement passes:** `ExceptionHandlingMiddleware` now correctly maps `ConcurrencyConflictException` → 409 (was blanket 500 before that fix). All 6 handlers also had the NotFound-severity bug (see below) and the blanket-catch-swallows-ConcurrencyConflictException bug, both fixed this pass to match PermitToWork/RiskManagement's precedent.

## NotFound severity bug — found and fixed

Same bug as RiskManagement: all 6 handlers' `new ApplicationError("AccidentManagement.Accident.NotFound", ..., request.AccidentId)` omitted `ErrorSeverity.NotFound`, so the default (`Error` → 500) applied. Fixed by adding the explicit severity argument on all 6.

## Tests

- `PlantSafety.Tests/AccidentManagement/Commands/AssignAccidentInvestigatorCommandHandlerTests.cs` — regression coverage mirroring `ScoreRiskCommandHandlerTests`: ETag forwarded to repository, `ConcurrencyConflictException` propagates.
- `PlantSafety.Tests/AccidentManagement/Handlers/GetAccidentByIdQueryHandlerTests.cs` — success+ETag, not-found (with severity assertion), cross-tenant isolation. Mirrors `GetRiskByIdQueryHandlerTests`.
- Pre-existing `PlantSafety.Tests/Application/AccidentManagement/AccidentInvestigationCommandHandlersTests.cs` updated for the `UpdateAsync` signature change; 3 pre-existing NotFound tests gained `ErrorSeverity.NotFound` assertions.
- Full unit suite verified green: 746/746 passing.
- **Also fixed as a side effect of this pass:** `PlantSafety.IntegrationTests` was left non-compiling by the RiskManagement pass (2-arg `UpdateAsync`/2-arg `GetByIdAsync` calls in `RiskConcurrencyAndAuditIntegrationTests.cs` and `RiskLifecycleIntegrationTests.cs` never updated for the new signatures) and the AccidentManagement pass introduced the identical break in `AccidentConcurrencyAndAuditIntegrationTests.cs` and `CorrectiveActionLifecycleIntegrationTests.cs`. All 14 compile errors across both modules' integration test files fixed (passing `eTag: null` / the required `tenantId` where the tests don't specifically exercise concurrency-conflict rejection). `PlantSafety.IntegrationTests` now builds with 0 errors. These tests were not executed (no Testcontainers/live SQL Server available in this session) — only confirmed to compile.

## Known remaining gaps (not addressed)

- `AccidentStatusResult`/`CreateAccidentCorrectiveActionResult` don't return a fresh ETag post-write, same limitation as RiskManagement/PermitToWork.
- `ListAsync` (Domain write-side) has no tenant scoping and no callers — same dormant gap as RiskManagement's.
- No `PlantSafety.DataIntegrationTests` coverage exercising the new `@ETag` guard or the fixed `GetById` TRY/CATCH against a live SQL Server.
- The 4 unrelated "category" read-only features under `PlantSafety.Application.AccidentManagement.*` (CorrectiveActionsCAPA, InvestigationRootCauseAnalysis, RegulatoryComplianceDocumentation, SiteSecuringAndTriage) were not audited this pass — same open question as RiskManagement's 11 category features.
