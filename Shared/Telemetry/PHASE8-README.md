# Phase 8 — Observability, Telemetry, Diagnostics, and Governance

## Scope (corrected before implementation)

The original Phase 8 prompt asked for a brand-new `Observability/` layer plus instrumentation for
Routing, Scheduling, and Diagnostics as if they were real modules inside this program. Neither
premise holds:

- `Shared.Telemetry` (Phase 2.4/3.3) already provides structured logging (`ILogEventWriter`/
  `LogEvent`/`LogCategory`/`LogSeverity`), metrics (`PlantSafetyMetrics`, `System.Diagnostics.Metrics`),
  and correlation (`ICorrelationContext`). Building a second, parallel system would have reproduced
  the exact "two disconnected systems" problem Phase 6 found and fixed for eventing
  (`IModuleEventGateway` vs. `Shared.EventBus`) — this time for telemetry.
- Routing, Scheduling, and Diagnostics have no execution path inside this program at all — no
  `RoutingEngine`, no scheduler, no Diagnostics namespace exists anywhere in this repository
  (verified by direct search, twice, before writing any code).

Phase 8 is therefore **instrumentation, not infrastructure**: it extends `Shared.Telemetry`'s
existing primitives into the real execution paths that exist — WorkflowEngine, Facility's
inference rules, and Audit's search — without inventing a second logging/metrics/tracing stack,
a scheduler, a dispatcher, or a caller for a module that has none today.

## What Was Built

### `Shared.Telemetry.Abstractions` (extended, not replaced)
- **`Metrics/PlantSafetyMetrics.cs`** — extended with `WorkflowDecisionsEvaluated`,
  `FacilityInferenceRuleEvaluations`, `AuditSearchesExecuted`, `AuditSearchDurationMs`,
  `ReliabilityFailures`, `ReliabilityOperationDurationMs`, `GovernanceViolationsDetected`. Same
  `Meter` instance as every pre-existing counter — no new metrics primitive.
- **`Reliability/ReliabilityHooks.cs`** — a single, generic `ExecuteAsync<TResult>` decorator used
  by every module's telemetry wrapper below. Records duration (via the injected `IClock`, never the
  wall clock) and outcome through the existing `ILogEventWriter`; on failure, rethrows the original
  exception unchanged. One implementation, not three copy-pasted try/catch blocks.
- **`Governance/TelemetryGovernanceRules.cs`** — pure validation functions (`ValidateLogEvent`,
  `ValidateMetricName`, `ValidateCorrelationPropagation`), zero side effects, returning a violation
  list. Governs *telemetry shape* only — no orchestration/scheduling/routing/diagnostics rules,
  because none of those have any real execution path to govern.
- **`Governance/TelemetryGovernanceEvaluator.cs`** — runs the rules above against a `LogEvent` and,
  on violation, writes one `TELEMETRY_GOVERNANCE_VIOLATION` log event plus a
  `GovernanceViolationsDetected` metric increment. Reuses `LogEvent` — no new event type.
- **`Events/LogCategory.cs` / `LogCategoryClassifier.cs`** — added an `Audit` category and fixed
  `WorkflowEngine`'s classifier to also match the unhyphenated `"workflowengine"` module string
  used by the new telemetry wrappers below.

### `Shared.WorkflowEngine.Infrastructure` (new project — WorkflowEngine previously had none)
- **`Telemetry/WorkflowEngineTelemetry.cs`** — wraps `WorkflowEngine.Evaluate` (Phase C.7, unchanged,
  still a pure function) with `ReliabilityHooks`, a `WorkflowDecisionsEvaluated` metric tagged by
  `SeverityLevel`, and a `WORKFLOW_DECISION_EVALUATED` log event. **This is the one real, live
  execution path instrumented in this phase** — `WorkflowEngineController.Evaluate` (a real HTTP
  endpoint, `POST /api/v1/ui/engines/workflow/evaluate`) now goes through this wrapper instead of
  calling `WorkflowEngine.Evaluate` directly. Verified live: `dotnet run` + `curl`, confirmed the
  decision is unchanged and both `WORKFLOW_DECISION_EVALUATED` and `RELIABILITY_OPERATION_SUCCEEDED`
  fire with the request's real correlation ID.

### `Shared.FacilityDomain.Infrastructure` (extended)
- **`Telemetry/FacilityTelemetry.cs`** — wraps Facility's three inference-rule engines
  (`HazardInferenceRules`, `SeverityInferenceRules`, `RegulatoryInferenceRules`; all unchanged, still
  pure) with the same `ReliabilityHooks` + logging + `FacilityInferenceRuleEvaluations` metric
  pattern. **Honest caveat, stated in the file's own header comment**: none of these three rules has
  a live caller anywhere in `PlantSafety.UI.Api` today — `FacilitiesController`/`FacilityService`
  supply `HazardType`/`SeverityLevel` directly from API callers rather than inferring them. This
  wrapper exists so the rules are instrumentation-ready the moment a real caller (e.g. one of the
  deferred AI.\* engines) is authorized to use them — it does not itself invent that caller.

### `Shared.AuditLogging.Infrastructure` (extended)
- **`Telemetry/AuditTelemetry.cs`** — `TelemetryAuditLogRepository`, a decorator implementing
  `IAuditLogRepository` around the real `AuditLogDapperRepository`, adding the same
  `ReliabilityHooks` pattern plus `AuditSearchesExecuted`/`AuditSearchDurationMs` metrics and an
  `AUDIT_SEARCH_EXECUTED` log event. Audit remains read-only — no new method was added to
  `IAuditLogRepository`. **Same honest caveat**: `IAuditLogRepository` has no live caller anywhere in
  `PlantSafety.UI.Api` today (no `AuditController` exists; `AddSharedAuditLogging` is never called
  from `Program.cs`) — this decorator is a drop-in replacement for whenever Audit search is wired
  into a real host, not a new caller itself.

## What Was Not Built (and why)

- **No new `Observability/` sink, logging primitive, metrics primitive, or tracing primitive.**
  `Shared.Telemetry` already provides all of these; this phase extends its usage.
- **No `RoutingTelemetry.cs`, `DiagnosticsTelemetry.cs`, `SchedulingTelemetry.cs`, or any file
  instrumenting Routing/Scheduling/Diagnostics.** None of these has any execution path inside this
  program — there is nothing to attach telemetry to. Confirmed by direct search (`grep -rln "class
  RoutingEngine"` and `grep -rln "namespace.*Diagnostics"` across the whole real program tree)
  before any code was written, not assumed.
- **No cross-module orchestration/scheduling/routing governance.** `TelemetryGovernanceRules`
  validates telemetry shape only, per the same "nothing to govern" finding above.
- **No new business logic in Facility, Employee, Machine, WorkflowEngine, or Audit.** Every wrapper
  in this phase is a decorator around an existing pure function or read-only repository method — none
  of their return values, side effects, or business rules changed.
- **Employee and Machine have no telemetry wrapper.** Per Phase 7's F0-04 finding (restated here,
  not re-litigated): Facility is the only module with real inference-rule engines to instrument in
  this shape. Employee/Machine's real execution paths (`EmployeeService`/`MachineService`'s command
  methods) already publish structured domain events via the Phase 4 domain passes — extending
  `ReliabilityHooks` around those methods as well is a natural, symmetric follow-up, not done in this
  pass to keep scope matched to what was explicitly requested (WorkflowEngine, Facility, Audit).

## Verification

- `dotnet build` — 0 errors across every touched/new project (`Shared.Telemetry.Abstractions`,
  `Shared.Telemetry.Tests`, `Shared.WorkflowEngine.Infrastructure` (new), `Shared.WorkflowEngine.Tests`,
  `Shared.FacilityDomain.Infrastructure`, `Shared.FacilityDomain.Tests`,
  `Shared.AuditLogging.Infrastructure`, `Shared.AuditLogging.Tests`, `PlantSafety.UI.Api`).
- Live verification: `dotnet run` against `PlantSafety.UI.Api`, `POST /api/v1/ui/engines/workflow/evaluate`
  with a Catastrophic/Fire context — confirmed the returned `WorkflowDecision` is byte-identical to
  calling `WorkflowEngine.Evaluate` directly, and that `WORKFLOW_DECISION_EVALUATED` and
  `RELIABILITY_OPERATION_SUCCEEDED` both appear in the structured JSON console log with the request's
  real `X-Correlation-Id`-derived correlation ID and correct trigger/action counts.
- Unit tests added: `ReliabilityHooksTests` (success/failure/deterministic-duration paths),
  `TelemetryGovernanceRulesTests` (valid/invalid log events, metric name convention, correlation
  propagation), `TelemetryGovernanceEvaluatorTests` (violation reporting), `WorkflowEngineTelemetryTests`
  (decision parity with direct `Evaluate`, log event shape), `FacilityTelemetryTests` (all three rule
  wrappers, result parity), `AuditTelemetryTests` (decorator success/failure paths, using a fake
  `IAuditLogRepository` — no live SQL Server needed, consistent with this program's established test
  pattern).
