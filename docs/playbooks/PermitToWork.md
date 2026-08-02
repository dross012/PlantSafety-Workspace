# PermitToWork Module Playbook

Phase 4 domain-modernization pilot module. Status as of 2026-07-29.

## Domain model

Four aggregate roots, each with its own bounded context under `PlantSafety.Domain.PermitToWork.<Sub>`:

| Aggregate | Purpose | Key invariants |
|---|---|---|
| `PermitWorkflow` | The end-to-end PTW lifecycle (Steps 1-7: Request → HazardAssessed → ControlsImplemented → Approved → Active → Closed → Archived, plus Rejected/Expired) | Guarded status transitions (`EnsureStatusIsOneOf`); required-controls/risk-score/reason validation on each step |
| `PermitTemplate` | Reusable permit type definitions (Draft → Published → Deprecated/Retired) | Guarded lifecycle transitions |
| `PermitScheduling` | Scheduling/conflict-detection for permitted work windows | Guarded lifecycle transitions |
| `PermitExpiration` | Expiration tracking and extension requests | Guarded lifecycle transitions |

All four are already rich aggregates (private setters, guarded factory `Create`, invariant-checked transition methods, raise domain events) — not anemic DTOs. Structure was sound going into this pass; the gaps found were in the surrounding Application/Infrastructure plumbing, not the aggregates themselves.

## Commands (Application layer)

`PermitToWork.PermitWorkflow`: Request, AssessHazards, ImplementControls, Approve, Issue, RecordClosureSignature, Archive, Reject.
`PermitToWork.PermitTemplate`: Publish, Deprecate.
`PermitToWork.PermitScheduling` / `PermitExpiration`: read-only today — no Application-layer mutating commands exist yet (only `Get<Sub>Query`). Adding Create/Update/Delete flows for these two is the largest remaining gap versus the Phase 4 checklist.

All 9 mutating commands now implement `IConcurrencyAwareCommand` and carry an `ETag`.

## Queries

One `Get<Sub>Query` per sub-aggregate, tenant/site/date-range scoped, returning a flat `<Sub>ItemDto`. DTOs now project `ETag` (from `GetById`/`GetRange`; `GetAllActive`'s in-memory-specification path does not, left as `null` there — low-value to change since nothing mutates off that path today).

## Events

Each sub-aggregate has its own event set (e.g. `PermitWorkflowCreatedEvent`, `...HazardsAssessedEvent`, `...ControlsImplementedEvent`, `...ApprovedEvent`, `...IssuedEvent`, `...ClosedEvent`, `...ArchivedEvent`, `...RejectedEvent`, `...ExpiredEvent`). Enqueued to the outbox (`IOutboxWriter`) for `Shared.EventBus` pickup by `TransactionalUnitOfWork.CommitAsync` post-handler.

**Fixed in this pass:** `UpdateAsync` in all 4 Dapper repositories was missing the outbox-enqueue block present in `AddAsync` — every post-creation event (Approve, Issue, Reject, Archive, Publish, Deprecate, etc.) was silently dropped and never reached the event bus. Now mirrors `AddAsync`.

## Stored procedures / repositories

Dapper repositories under `PlantSafety.Infrastructure.PermitToWork.<Sub>.Persistence`, backed by `usp_<Sub>_{Insert,Update,GetById,GetRange,GetAllActive}` in the `[PermitToWork]` SQL schema. TRY/CATCH + AuditLog + ErrorLog (Phase 3 canonical pattern) was already present.

**Added in this pass:** optimistic concurrency. Each table gained a native `RowVersion ROWVERSION` column (additive `ALTER TABLE` guard for already-deployed environments; the old `ConcurrencyToken UNIQUEIDENTIFIER` column is left in place, unused). `Update` procs now guard `WHERE ... AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag)`, capture `@RowsAffected` before the audit INSERT, and return it via `SELECT` — callers must invoke via `ExecuteScalarAsync`, not `ExecuteNonQuery`, or the check silently no-ops. Zero rows affected → `ConcurrencyConflictException`. Pattern mirrors `SafetyEvents.IncidentManagement`'s existing implementation exactly (no new pattern invented).

## API

`PermitToWorkV1Controller` — thin MediatR dispatcher, already API-versioned, policy-authorized per action, standard `ApiResponse<T>` envelope. Mutating request contracts now carry `ETag`.

**Fixed in this pass (platform-wide, not just this module):** `ExceptionHandlingMiddleware` unconditionally returned HTTP 500 for every unhandled exception, even though `ConcurrencyConflictException`, `ApplicationAuthorizationException`, and `DomainRuleException` all documented specific status codes (409/403/400 respectively) in their own XML doc comments — those mappings were never wired up. Now dispatches to the existing (but previously unused) `ProblemDetailsExtensions.ConflictProblem` / `ForbiddenProblem` / `ValidationProblem` builders. This was a documented-but-unimplemented gap affecting every module that raises these exception types, not unique to PermitToWork.

Also fixed: the 9 PermitToWork command handlers wrapped their entire body in `catch (Exception ex)`, which would have swallowed a `ConcurrencyConflictException` into a generic 400 `ApplicationError` before it ever reached the middleware. Added a `catch (ConcurrencyConflictException) { throw; }` ahead of the generic catch in each.

## Tests

`PlantSafety.Tests/PermitToWork/Commands/ApprovePermitWorkflowCommandHandlerTests.cs` — regression coverage for both fixes above: (1) the handler forwards the caller's ETag to the repository, (2) a `ConcurrencyConflictException` from the repository propagates unchanged rather than being swallowed. Dapper-repository-level behavior (outbox enqueue, SQL concurrency guard) is not independently unit-tested — this codebase doesn't unit-test Dapper repositories directly (they need a live SQL connection); that coverage belongs in `PlantSafety.IntegrationTests`, which does not yet have a PermitToWork suite.

## Known remaining gaps (not addressed in this pass — flagged, not silently dropped)

- No Create/Update/Delete Application-layer commands for `PermitScheduling`/`PermitExpiration` (read-only today).
- `PermitWorkflowActionResult`/`PermitTemplateActionResult` don't return a fresh ETag post-write; API clients must re-GET to obtain the new ETag before their next mutation.
- No `PlantSafety.IntegrationTests` coverage for this module (concurrency-conflict-over-the-wire, event-consumption).
- `BulkUpdatePermitToWorkCommand` (the `/bulk` endpoint) is a separate legacy bulk-write path, intentionally left untouched — worth a follow-up pass to decide whether it should route through the same aggregate/event/concurrency path or stay a distinct bulk operation.
