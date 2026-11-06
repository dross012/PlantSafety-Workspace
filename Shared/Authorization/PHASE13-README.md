# Phase 13 — Routing Modernization (retargeted from "Diagnostics Modernization")

## Scope correction: Diagnostics does not exist in this program

The original Phase 13 prompt asked to modernize "Diagnostics" into a full domain module — rule
engine, inference engine, orchestrator, scheduler, router, DTOs, authorization, authentication,
tenancy, telemetry, governance. Before writing any code, this was checked directly and confirmed
false:

- `git ls-files` from the outer repo returns zero Diagnostics controllers, consumers, or domain
  files anywhere in this program.
- `docs/ingestion/module-flow/diagnostics.md`, written during an earlier investigation phase of
  this same program, already documents the finding explicitly: "Diagnostics has zero API
  controllers or minimal-API endpoints... zero message-broker/event-consumer classes, zero
  file-ingestion adapters, and zero scheduled-job/hosted-service classes anywhere in the module...
  it is excluded from the flow-map... because its absence of ingress was verified, not overlooked."
- The standalone product at `C:\Users\dross\source\Diagnostics` is a different codebase under a
  different repo root entirely, unrelated to `D:\PlantSafety\source`.

Building `DiagnosticsRuleEngine`/`DiagnosticsOrchestrator`/`DiagnosticsInferenceEngine`/etc. from
nothing would have meant inventing an entire fictional subsystem, directly violating every phase's
own standing constraint ("no invented business logic"). This was raised with the user via
`AskUserQuestion` before writing any code (the same pattern used for Phases 5/6/8/9's genuinely
novel/high-risk scope mismatches). The user's answer retargeted Phase 13 to **Routing**.

## Verifying Routing is real (not repeating the same mistake)

The user's corrective answer asserted Routing "has real domain models... real RequiredAction
semantics... real integration with WorkflowEngine... real business meaning." Given this session's
established pattern of the user's own corrective answers sometimes containing inaccuracies (Phase
9's `SafetyRoles.System`/`PermissionSet.All`), this was independently verified before proceeding,
not taken on faith:

- `Shared.SafetyData.Abstractions.Models.Industries.Eventing.EventRoutingEngine.Route(EventDefinition,
  FacilityProfile) -> EventRoutingResult` (Phase C.8, already merged) is real, complete, and
  genuinely deterministic: it projects/concatenates its inputs (activated programs, SOPs, training,
  audits, permits, regulators, dashboard alerts, compliance elements, event-bus topics) with no
  branching on probability or weight — literally the "additive accumulation, no cancellation rules"
  pattern this phase's instructions ask for, already built.
- `IEventRoutingService`/`EventRoutingService` (Phase C.8) is a real DI-facade over it, registered
  via `EventingServiceCollectionExtensions.AddEventing()` — which is **not** called from
  `PlantSafety.UI.Api`'s `Program.cs` today, so (like Facility's inference rules and Audit's search
  before it) this is real, dormant capability, not a live endpoint.
- `RequiredAction` is real (`WorkflowDecision.RequiredActions`, Phase 4/8). `NotificationRouting`
  is real (`WorkflowDecision.NotificationRoutings`, Phase C.7, already observed live in this
  session's Phase 10/11 `curl` verification output).

What was **not** found, and therefore not built: any due-time/scheduling metadata on
`RequiredAction` (no scheduling semantics exist to modernize — this phase does not introduce a
`DiagnosticsScheduleCalculator`-equivalent), and no live dispatcher for routing decisions (matching
the prompt's own "not create a dispatcher" instruction).

## What was built

**`Shared.Telemetry.Abstractions`**
- `LogCategory.Routing` added (mirrors `Authentication`/`Tenant` from Phases 10/11);
  `LogCategoryClassifier` extended; `PlantSafetyMetrics.RoutingDecisionsComputed` counter added.

**`Shared.SafetyData.Infrastructure`** (gained its first dependency on `Shared.Deterministic`,
`Shared.Telemetry`, and `Shared.Authorization` — previously only referenced `Shared.SqlGateway`)
- `Telemetry/RoutingTelemetry.cs` — wraps `EventRoutingEngine.Route` with structured logging
  (`ROUTING_DECISION_COMPUTED`) + the new `RoutingDecisionsComputed` metric. Mirrors
  `WorkflowEngineTelemetry` (Phase 8) exactly; the routing decision itself is unchanged.
- `Authorization/RoutingAuthorization.cs` — wraps `RoutingTelemetry` with a deterministic
  `"Routing.Read"` permission check (`ModulePermissionVerb.Read` — the correct existing verb for a
  read-only computation, not an invented one). Mirrors `WorkflowEngineAuthorization` (Phase 9)
  exactly, including the `UnauthorizedAccessException` → `ErrorCategory.Unauthorized` → HTTP 401
  reuse.
- `Tenant/RoutingTenant.cs` — wraps `RoutingAuthorization` with a tenant-isolation check
  (`TenantIsolationCheck`) against `facility.FacilityId`, and composes a `ContractMetadata` (Phase
  12) via `ContractMetadataFactory`, running `ContractGovernanceEvaluator`. Mirrors
  `WorkflowEngineTenant`/`FacilityTenant` (Phase 11) — with one difference worth noting: this is
  the **first** module-level tenant wrapper in this program where the facility-boundary branch of
  `TenantIsolationCheck` is exercised with a real `Guid`, not a dormant `null` — every other module
  (`WorkflowContext`, `AuditLogSearchCriteria`) carries no facility field at all, but
  `FacilityProfile.FacilityId` (the actual input to `EventRoutingEngine.Route`) is real.

## What was deliberately not built

- No `DiagnosticsRuleEngine`/`DiagnosticsInferenceEngine`/`DiagnosticsOrchestrator`/
  `DiagnosticsEvaluationPipeline`/`DiagnosticsEventEmitter` — no Diagnostics domain exists to
  modernize.
- No `RoutingScheduleCalculator`/`RoutingDueTimeEvaluator`/`RoutingScheduledEvent` — no scheduling
  metadata exists on `RequiredAction` or anywhere in the routing pipeline to formalize.
- No `RoutingRuleEngine`/`RoutingRoutingDecision` as new types — `EventRoutingEngine.Route` /
  `EventRoutingResult` already are the deterministic rule engine and its decision output; wrapping
  them (as above) is the correct, non-duplicative move, consistent with Phase 12's own "don't
  create a parallel system" finding.
- No dispatcher — explicitly out of scope per the prompt's own instruction ("Phase 15 will").

## Verification performed

- `Shared.SafetyData.Infrastructure` and `PlantSafety.UI.Api` build cleanly (0 errors); adding the
  new project references did not affect any existing consumer.
- `Shared.SafetyData.Tests` — 217/217 pass, including 12 new `Telemetry`/`Authorization`/`Tenant`
  tests for the Routing wrappers (matching-direct-result, denial, cross-facility-denial,
  resolver-grant/deny cases) alongside every pre-existing `Shared.SafetyData` test, unaffected.
- Not wired into any live endpoint: consistent with `EventRoutingEngine`'s own dormant status
  (confirmed above) and with how Phase 8/9 left `FacilityTelemetry`/`FacilityAuthorization` dormant
  for the same reason — there is no live Routing-facing controller in `PlantSafety.UI.Api` to wire
  into today.
