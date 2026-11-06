# Phase 12 — Unified Contract & Schema Modernization

## Scope corrections made before writing any code

1. **Diagnostics/Routing/Scheduling reintroduced (6th consecutive occurrence).** Dropped again —
   still nonexistent. No unified-event or unified-DTO wiring was written for them.
2. **"CallerPermissions" reintroduced as required metadata (2.3).** Dropped again — there is no
   `Permissions` field anywhere in `Shared.Authorization` (confirmed in Phases 9/10/11).
   `ContractMetadata` carries `CallerRole`, `CorrelationId`, `MfaStatus`, `TenantId`,
   `TimestampUtc` — permissions remain resolved on demand via `IPermissionResolver`.
3. **This program already has unified event, DTO, and error schemas — they were not duplicated.**
   Before writing any code, each was checked directly:
   - **Events**: `Shared.Telemetry.Abstractions.Events.LogEvent`/`ILogEvent` (Phase 2.4) is already
     the one event shape every module — WorkflowEngine, Facility, Audit, Authorization,
     Authentication, Tenant, Governance — constructs (`EventId, Category, Severity, Message,
     Payload, CorrelationId, Module, OccurredAtUtc`). Verified by inspecting every `*Telemetry.cs`
     file across Phases 8–11: all of them build the same record.
   - **DTOs**: `WorkflowEngineController.cs`'s own Phase-3 comment already establishes the rule —
     domain contracts (`WorkflowContext`/`WorkflowDecision`) are used directly as request/response
     DTOs, no separate DTO layer exists or was ever supposed to. There is nothing to "unify" that
     isn't already one shape.
   - **Errors**: `Shared.ErrorHandling.Abstractions.Envelope.ErrorEnvelope` (Phase 2.3) is already
     `ErrorBase`+`ErrorMetadata`+`ErrorCategory` — `ErrorCode, Category, UserMessage,
     DeveloperMessage, CorrelationId, Module, OccurredAtUtc`, with `ErrorCategory` already a closed
     enum (`Validation/NotFound/Conflict/Unauthorized/Dependency/Internal`) and
     `ExceptionClassifier` already mapping `UnauthorizedAccessException -> Unauthorized`, exactly
     what section 1.4 asks for.

   Introducing new `UnifiedEventBase`/`UnifiedDtoBase`/`UnifiedErrorBase` types alongside these
   would be the exact "parallel system" this program's own precedent (Phase 6's
   `IModuleEventGateway`-vs-`Shared.EventBus` discovery) already taught it to avoid, and this
   phase's own instructions explicitly forbid ("never create a parallel telemetry system"). None
   were written. What *was* missing — and what this phase actually builds — is documented below.
4. **`ContractBase` is not retrofitted onto existing records.** `CallerContext`, `TenantContext`,
   `WorkflowContext`, `WorkflowDecision`, `AuditLogSearchCriteria`, `ErrorEnvelope`, and every other
   already-merged, already-tested record from Phases 2–11 keep their existing constructor shape.
   Changing their base type or field order would be a breaking signature change to code already
   relied on across dozens of call sites, for no functional benefit — the same "must not introduce
   new business logic" constraint this phase's own instructions impose. `IContract`/`ContractMetadata`
   are new, **opt-in** types for future contracts to converge on from the start, not a rewrite of
   the past eleven phases.
5. **Deterministic (alphabetical) field ordering applies only to the new types this phase
   introduces**, not retroactively to existing positional records — reordering
   `CallerContext(Role, MfaStatus, CorrelationId)` or `TenantContext(TenantId, FacilityId, Caller,
   CorrelationId)` alphabetically would break every already-merged call site for a cosmetic
   convention. `ContractMetadata`'s five properties (`CallerRole, CorrelationId, MfaStatus,
   TenantId, TimestampUtc`) are alphabetical because it is a brand-new type with nothing to break.

## What was actually missing, and what this phase builds

Given the above, real unification gaps in this program were: (a) no single bundle composing
tenant + caller + correlation + timestamp together, (b) nothing validated that a `LogEvent`'s
correlation id was actually consistent with the request metadata that produced it, (c) nothing
validated an `ErrorEnvelope`'s own shape (only `ExceptionClassifier` mapped exception types), and
(d) the identity/tenant/contract governance evaluators from Phases 9–12 each ran independently, with
no single composed check. This phase builds exactly those four things:

**`Shared.Authorization.Abstractions/Contracts/`**
- `ContractMetadata.cs` — the composed bundle, `SchemaVersion` const (=1).
- `IContract.cs` — opt-in marker interface for future contracts.
- `ContractValidationRules.cs` — pure shape validation.

**`Shared.Authorization.Infrastructure/Contracts/`**
- `ContractMetadataFactory.cs` — pure `Create(TenantContext, IClock) -> ContractMetadata`.

**`Shared.Authorization.Infrastructure/Governance/`**
- `ContractGovernanceEvaluator.cs` — reports `ContractValidationRules` violations (same
  `GovernanceViolationsDetected` counter/`ILogEventWriter` pattern as every prior phase).
- `SchemaGovernanceRules.cs`/`SchemaGovernanceEvaluator.cs` — the composed check: runs
  `AuthenticationGovernanceRules.ValidateResolution` (Phase 10) +
  `TenantGovernanceRules.ValidateResolution` (Phase 11) + `ContractValidationRules.Validate`
  (this phase) together and reports once. It calls those rule sets; it does not duplicate them.

**`Shared.Authorization.Infrastructure/Events/`**
- `UnifiedEventGovernance.cs`/`UnifiedEventTelemetry.cs` — pure + reporting check that a given
  `ILogEvent`'s `CorrelationId` matches the `ContractMetadata` of the request that produced it.
  Operates on the existing `LogEvent`/`ILogEvent`; no new event record.

**`Shared.Authorization.Infrastructure/Telemetry/`**
- `ContractTelemetryExtensions.cs` — `ToLogPayload()` (alphabetical keys) and `RecordEmitted(...)`
  extension methods on `ContractMetadata`, reusing `ILogEventWriter`.

**`Shared.ErrorHandling.Infrastructure`** (new project — previously only Abstractions/Tests existed,
matching every other module's Abstractions/Infrastructure split)
- `Governance/ErrorGovernanceRules.cs`/`ErrorGovernanceEvaluator.cs` — pure + reporting shape
  validation of an `ErrorEnvelope` (non-empty `ErrorCode`/`CorrelationId`/`Module`, non-default
  `OccurredAtUtc`, `Category` is one of the closed `ErrorCategory` values — never a new one).
- `Telemetry/ErrorTelemetryExtensions.cs` — `ToLogPayload()` (alphabetical keys) for an
  `ErrorEnvelope`.

**`Shared.Telemetry.Abstractions`** — no new types; `LogCategory`/`LogCategoryClassifier`/
`PlantSafetyMetrics` already cover every category this phase touches (`Infrastructure` is reused
for contract/event/schema governance events, matching how cross-cutting infrastructure concerns are
already categorized).

**`PlantSafety.UI.Api`**
- `Middleware/ApiKeyAuthenticationMiddleware.cs` — extended (additively) to compose the request's
  already-resolved `CallerContextResolution`/`TenantContextResolution` into a `ContractMetadata`
  via `ContractMetadataFactory`, record it (`RecordEmitted`), and run
  `SchemaGovernanceEvaluator.EvaluateAndReport` once per request — the single composed
  identity/contract/tenant governance pass, on top of (not instead of) the per-layer checks Phases
  10/11 already run.

## Why error-path governance is not composed into SchemaGovernanceEvaluator

`ErrorGovernanceEvaluator` validates `ErrorEnvelope` shapes at the exception-handling boundary
(`GlobalErrorHandler`, Phase 2.3) — a different lifecycle from the success-path request metadata
`SchemaGovernanceEvaluator` validates in the authentication middleware. Composing them would require
`Shared.Authorization` to reference `Shared.ErrorHandling` for a check that never runs on the same
request path as the rest of the composition. They remain two independent evaluators, each wired at
its own natural boundary.

## Verification performed

- `Shared.Authorization.Abstractions`, `Shared.Authorization.Infrastructure`,
  `Shared.ErrorHandling.Infrastructure` (new), and `PlantSafety.UI.Api` all build cleanly (0
  errors).
- Full test suites pass: `Shared.Authorization.Tests` 100/100 (22 new Phase 12 tests alongside
  every pre-existing test, unaffected), `Shared.ErrorHandling.Tests` 38/38 (new
  `Governance`/`Telemetry` coverage alongside every pre-existing test).
- Live-verified via `dotnet run` + `curl` against `POST /api/v1/ui/engines/workflow/evaluate` with
  no API key: `200 OK`, identical response to before this phase. `CONTRACT_METADATA_EMITTED` logs
  fire with `TenantId=__unscoped__`, `CallerRole=Platform.Admin`, `MfaStatus=NotApplicable`,
  correctly populated `CorrelationId`/`TimestampUtc`/`SchemaVersion`; no `SCHEMA_GOVERNANCE_VIOLATION`
  (the resolution is well-formed) — confirming the new composed governance pass runs on every
  request without changing observable behavior.
