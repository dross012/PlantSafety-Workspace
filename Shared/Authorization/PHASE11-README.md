# Phase 11 — Multi-Tenant & Contextual Access Modernization

## Scope corrections made before writing any code

1. **Diagnostics/Routing/Scheduling reintroduced (5th consecutive occurrence).** Dropped again —
   confirmed still nonexistent anywhere in this program's real code. No `DiagnosticsTenant.cs`,
   `RoutingTenant.cs`, or `SchedulingTenant.cs` was written — there is nothing to wrap.
2. **`DepartmentId`/`MachineGroupId` do not exist anywhere in the domain.** Searched
   `Shared.FacilityDomain`, `Shared.EmployeeDomain`, `Shared.MachineDomain`, and `Shared.SafetyData`
   before writing `TenantContext` — neither concept exists, implicitly or explicitly, anywhere.
   `TenantContext` carries only `TenantId` and `FacilityId`, matching the real, already-committed
   `TenantFacilityMap` (Phase C.8, `Shared.SafetyData`) exactly: `(TenantId: string, FacilityId:
   Guid)`. Adding the other two fields would invent business concepts this phase's own instructions
   ("must not introduce new business logic... formalize what already exists implicitly") explicitly
   forbid.
3. **A real multi-tenant model already exists and was reused, not reinvented.**
   `Shared.SafetyData.Abstractions.Models.Industries.Orchestration.ITenantFacilityRegistryV2`
   (Phase C.8) is the authoritative tenant/facility binding registry, already registered in
   `PlantSafety.UI.Api`'s `Program.cs`. `TenantContext`/`TenantIsolationCheck` formalize access
   *around* that existing data through `ITenantFacilityAccessResolver` (see below) rather than
   building a second, competing tenant model.

## Why TenantContext doesn't depend on Shared.SafetyData directly

`Shared.Authorization` has no dependency on `Shared.SafetyData` today, and adding one would be a new,
backwards dependency (authorization depending on safety-data). Instead, `TenantIsolationCheck` is
parameterized over `ITenantFacilityAccessResolver` — a one-method seam (`CanAccessFacility(tenantId,
facilityId)`) defined in `Shared.Authorization.Abstractions.Tenant`. The composition root
(`PlantSafety.UI.Api`) supplies the real implementation, `TenantFacilityAccessResolver`, which simply
adapts `ITenantFacilityRegistryV2.CanRouteToFacility`. `Shared.Authorization`'s dependency graph is
unchanged; the real tenant/facility data lives in exactly one place.

## TenantContext.Unscoped — this phase's equivalent of CallerContext.SystemDefault

No live caller sends a tenant-bound API key today (same standing fact as Phase 10's "no live caller
sends any API key"). `TenantContext.Unscoped(caller, correlationId)` is the deterministic fallback —
`TenantId = "__unscoped__"`, `FacilityId = null` — and `TenantIsolationCheck` always allows an
Unscoped context: there is no tenant boundary configured yet, so there is nothing to enforce. This is
documented honestly (in `TenantContext.cs`'s own header) as the literal absence of configuration data,
not a privileged bypass invented for convenience — the same reasoning `CallerContext.SystemDefault`
(Phase 9) already established for `PlatformAdmin`-by-default.

## Why requestedFacilityId is threaded as an explicit parameter, not read off request DTOs

Before writing any wrapper, the real request shapes were checked:

- `WorkflowContext` (`Shared.WorkflowEngine.Abstractions.Engine`) carries `HazardType`,
  `SeverityLevel`, `EvacuationScope`, and three counts — **no facility or tenant field**.
- `AuditLogSearchCriteria` (`Shared.AuditLogging.Abstractions.Models`) carries table/operation/actor/
  module/severity/date-range/text filters and pagination — **no facility or tenant field**.

Neither request contract was extended with a new field: doing so would be inventing new business
logic/schema this phase's instructions explicitly forbid, and would touch already-merged, tested
contracts from Phases 4–10 for no operational benefit (no live caller supplies that data today).
Instead, every tenant-aware wrapper takes `Guid? requestedFacilityId` as an explicit parameter.
`TenantIsolationCheck` treats `null` as "not facility-scoped; allow" — real, tested, deterministic
behavior, wired into the one live endpoint with `null` today, ready to receive a real value the moment
a request DTO actually carries one. This mirrors Phase 8's own precedent: `FacilityTelemetry` and
`AuditTelemetry` were built as real, tested, wired capability despite having no live caller in
`PlantSafety.UI.Api` at the time — dormant-but-real, not invented-and-pretended-live.

## What was built

**`Shared.Authorization.Abstractions/Tenant/`**
- `TenantContext.cs` — immutable record (`TenantId`, `FacilityId?`, `Caller`, `CorrelationId`),
  `SchemaVersion` constant (versioned, per the prompt's requirement), `Unscoped()` factory.
- `TenantAccessDecision.cs` — mirrors `AuthorizationDecision` (Phase 9) exactly.
- `ITenantFacilityAccessResolver.cs` — the seam described above.
- `TenantAuthenticationOptions.cs` — config-bound API key → TenantId map, additive to (independent
  of) Phase 10's `ApiKeyAuthenticationOptions` role map.
- `TenantContextResolution.cs` / `TenantContextFactory.cs` — mirror `CallerContextResolution`/
  `CallerContextFactory` (Phase 10) exactly: pure composition, falls back to `Unscoped` on
  missing/unrecognized keys.
- `TenantIsolationCheck.cs` — pure `Evaluate(tenantContext, requestedFacilityId, resolver)`. Rule
  order: Unscoped → allow; null facility → allow; context has its own scoped `FacilityId` → exact
  match required; otherwise consult the resolver.
- `ITenantContextProvider.cs` / `TenantContextProvider.cs` — mirror `ICallerContextProvider`/
  `CallerContextProvider` (Phase 10) exactly: scoped, Set-once-then-read.

**`Shared.Authorization.Infrastructure`**
- `Telemetry/TenantTelemetry.cs` — `RecordResolution`/`RecordIsolationDecision`, reusing
  `ILogEventWriter`/`PlantSafetyMetrics` (two new counters: `TenantResolutionsPerformed`,
  `TenantIsolationChecksPerformed`/`TenantIsolationDenials`).
- `Telemetry/TenantTracing.cs` / `TenantCorrelation.cs` — thin named wrappers, same pattern as
  `AuthenticationTracing`/`AuthenticationCorrelation` (Phase 10).
- `Governance/TenantGovernanceRules.cs` / `TenantGovernanceEvaluator.cs` — pure validation
  (`ValidateResolution`, `ValidateTenantContext` for domain-wrapper call sites that only have the
  bare context, `ValidateIsolationDecisionShape`), reported through the existing
  `GovernanceViolationsDetected` counter — the same mechanism Phases 8/9/10 already established.

**`Shared.Telemetry.Abstractions`**
- `LogCategory.Tenant` added (mirrors Phase 10's `Authentication` addition); `LogCategoryClassifier`
  extended; `PlantSafetyMetrics.TenantResolutionsPerformed`/`TenantIsolationChecksPerformed`/
  `TenantIsolationDenials` counters added.

**Module-level tenant enforcement (real, wired, tested — dormant where the underlying module already was)**
- `Shared.WorkflowEngine.Infrastructure/Tenant/WorkflowEngineTenant.cs` — wraps
  `WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync` (Phase 9, unchanged) with a
  tenant-isolation check. **Wired into the one live endpoint.**
- `Shared.FacilityDomain.Infrastructure/Tenant/FacilityTenant.cs` — wraps `FacilityAuthorization`'s
  three inference-rule methods. Dormant (no live caller — same as Phase 8/9's own documented caveat).
- `Shared.AuditLogging.Infrastructure/Tenant/AuditTenant.cs` (`TenantScopedAuditLogRepository`) —
  decorates `IAuditLogRepository` (over `AuthorizedAuditLogRepository`, Phase 9, unchanged). Dormant,
  same reason.

**`PlantSafety.UI.Api`**
- `Tenant/TenantFacilityAccessResolver.cs` — the composition-root adapter described above.
- `Middleware/ApiKeyAuthenticationMiddleware.cs` — extended (additively) to also resolve
  `TenantContext` from the same `X-Api-Key` header against the independent tenant-key map, and
  populate the new `TenantContextProvider`.
- `Program.cs` — binds `TenantAuthenticationOptions`, registers `ITenantFacilityAccessResolver` and
  the scoped `TenantContextProvider`/`ITenantContextProvider`.
- `Controllers/WorkflowEngineController.cs` — `Evaluate` now calls
  `WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync`, reading `TenantContext` from
  `ITenantContextProvider.Current` and passing `requestedFacilityId: null` (see above).

## What was deliberately not built

- No `DepartmentId`/`MachineGroupId` fields, no Diagnostics/Routing/Scheduling wrappers — see
  corrections above.
- No dynamic tenant/role/permission creation, no probabilistic/weighted/averaging logic, no
  cancellation rules, no side effects — `TenantIsolationCheck` and `TenantContextFactory` are pure,
  deterministic functions, same as every decision function since Phase 9.
- No new roles or permissions — tenant enforcement is a boundary check *in addition to* Phase 9's
  role/permission check, not a replacement or an alternate permission system.
- No change to `WorkflowContext`, `AuditLogSearchCriteria`, or any other already-merged request
  contract — see "Why requestedFacilityId is threaded as an explicit parameter" above.

## Verification performed

- `Shared.Authorization.Abstractions`, `Shared.Authorization.Infrastructure`,
  `Shared.WorkflowEngine.Infrastructure`, `Shared.FacilityDomain.Infrastructure`,
  `Shared.AuditLogging.Infrastructure`, and `PlantSafety.UI.Api` all build cleanly (0 errors).
- Full test suites pass: `Shared.Authorization.Tests` 78/78, `Shared.WorkflowEngine.Tests` 21/21,
  `Shared.FacilityDomain.Tests` 116/116, `Shared.AuditLogging.Tests` 13/13 — including all new Phase
  11 coverage alongside every pre-existing test (unaffected).
- Live-verified via `dotnet run` + `curl` against `POST /api/v1/ui/engines/workflow/evaluate`:
  - No `X-Api-Key` header: `200 OK`, identical response to before this phase. `TENANT_RESOLVED` logs
    `Outcome=NoKeyProvided`, `TenantId=__unscoped__`; `TENANT_ISOLATION_CHECK_PERFORMED` logs
    `Outcome=Allow`, reason "No tenant binding configured... no boundary to enforce."
  - A key registered via `TenantAuthentication:RegisteredKeyTenants` config (mapped to
    `acme-plant-1`, alongside a Phase 10 role mapping on the same key): `200 OK`,
    `TENANT_RESOLVED` logs `Outcome=Recognized`, `TenantId=acme-plant-1` — the resolved tenant is
    correctly threaded through the isolation check (which allows, since `requestedFacilityId` is
    `null` at this call site today) and the request succeeds identically.
