# Lifecycle Coverage — Intelligence

Phase 6 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/intelligence.md`, `gaps/orphaned-ingress.md`, `gaps/missing-events.md`,
`gaps/tenant-violations.md` (Phase 3-4), plus direct, full reads this phase (marked **[NEW]**) of
`Intelligence.Domain.AITraining.Aggregates.TrainingJob`, `Intelligence.Domain.AIInvestigations
.Aggregates.Investigation`, their command handlers, and their repository implementations.

## [NEW] Headline finding: Intelligence has real, well-guarded domain aggregates that the running code never touches

This is the single most important finding in this document and arguably the most important
"asymmetric lifecycle" finding across the whole Phase 6 discovery, because it inverts the pattern
found everywhere else: it is not that a transition is unguarded — **it is fully guarded in a domain
class that the application layer never calls.**

- `TrainingJob` (`Intelligence.Domain.AITraining.Aggregates.TrainingJob.cs`, full read) implements a
  genuinely correct state machine: `Submit` (→Pending), `RegisterDataset` (requires non-terminal,
  rejects double-registration), `Start` (requires Pending **and** a registered dataset — a real
  cross-field invariant, not just a status check), `Complete` (requires Running), `Cancel` (rejects
  if `Status.IsTerminal`, requires non-empty reason), `RecordEvaluation` (rejects if terminal,
  requires non-empty summary), `Fail` (rejects if terminal). Every transition raises a
  correctly-shaped domain event (`TrainingJobStartedEvent`/`RunningEvent`/`CompletedEvent`/
  `CancelledEvent`/`EvaluatedEvent`/`FailedEvent`).
- **[NEW]** Grepping all of `Intelligence.Application`/`Intelligence.Api` for any call to
  `TrainingJob.Submit(...)`, `new TrainingJob(...)`, `.Cancel(...)`, `.Complete(...)`, `.Start(...)`,
  or `.Fail(...)` on this type returns **zero results outside the aggregate's own file and its unit
  tests.** Every one of the three handlers this phase read in full
  (`StartTrainingJobCommandHandler.cs`, `CancelTrainingJobCommandHandler.cs`,
  `EvaluateTrainingJobCommandHandler.cs`) calls `ITrainingJobRepository.CreateAsync`/`CancelAsync`/
  `RecordEvaluationAsync` **directly against a Dapper stored procedure**
  (`intelligence.usp_TrainingJob_Create`/`_Cancel`/`_RecordEvaluation`), without ever constructing or
  loading a `TrainingJob` instance. The aggregate's guard clauses (terminal-state checks,
  dataset-required-before-start, non-empty-reason checks) **never execute at runtime** — they exist
  in source, are presumably unit-tested in isolation, and have no path by which a real HTTP request
  ever reaches them.
- **[NEW]** The exact same pattern was confirmed for `Investigation`
  (`Intelligence.Domain.AIInvestigations.Aggregates.Investigation.cs`): `Open`, `AddFinding`,
  `Cancel` (rejects terminal state, requires non-empty reason), `SubmitHypothesis`, `Complete` are
  all guarded — and `CancelInvestigationCommandHandler.cs` (full read) calls
  `_repository.CancelAsync(...)` directly, never touching the `Investigation` class.
- **Consequence for the "Domain-Rule-Enforced?" question this phase's table has to answer**: for
  every one of these transitions, the honest answer is **"guarded in a class that is never invoked —
  effectively No at runtime, whatever the SQL stored procedure itself may or may not additionally
  enforce."** Whether `usp_TrainingJob_Cancel`/`usp_Investigation_Cancel` themselves have a
  `WHERE Status NOT IN (...)` guard could not be checked this phase — Intelligence, unlike
  PlantSafety, does not appear to keep its SQL objects under source control in this repository (no
  `.sql` files were found for these procedures), so their contents are unverifiable from the codebase
  alone. **This should be flagged for Phase 7 as a concrete, high-value follow-up**: if the stored
  procedures also lack a state guard, then Intelligence training jobs and investigations can be
  cancelled/completed/failed in any order with zero enforcement anywhere in the stack, despite a
  domain model that reads as if this were handled.

## AITraining (TrainingJob)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| TrainingJob | Submit (Create) | Yes — via `StartTrainingJobCommandHandler` → `_repository.CreateAsync` | **No at runtime** — the domain factory's guards (non-empty `modelName`/`modelVersion`) are never exercised; only whatever the command's own FluentValidation (not opened this phase) and the SQL proc itself enforce | **No** — `TrainingJobStartedIntegrationEvent`/`TrainingJobStartedEvent` never instantiated by the live handler (Phase 3, unchanged; confirmed again this phase — handler has no `IPublisher`/event reference) | Unverified — no cross-DB AuditLog mechanism found in Intelligence (unlike PlantSafety) | Partial — **[NEW]**: `TrainingJob` the aggregate has **no `TenantId` property at all** (full read confirms the property list: ModelName, ModelVersion, Status, Dataset, AccuracyScore, timestamps, SubmittedByActorId, FailureReason — no tenant field). Whatever tenant scoping exists is presumed to live only in the command/repository layer, not the domain model | Directly verified **[NEW]** | Compounds the already-known Intelligence Outbox tenant gap (`gaps/tenant-violations.md` §4) — the aggregate itself, not just the outbox row, has no tenant concept |
| TrainingJob | RegisterDataset | Domain method exists; **no command/handler found this phase that calls it** — not confirmed whether any live path reaches it | N/A — unreached | N/A | Unverified | N/A | Directly verified (aggregate); pattern-inferred (no caller found) | Possible additional dead domain method beyond the three confirmed bypassed ones |
| TrainingJob | Start (domain: Pending→Running, requires dataset) | The **domain transition** exists but is never invoked; the **live** "start" path is `StartTrainingJobCommandHandler` calling `CreateAsync` directly — it is unclear from this phase's evidence whether "Start" as a distinct HTTP action even exists, or whether Create-and-Start are conflated at the SQL layer | **No at runtime** (see headline finding) | No | Unverified | Same tenancy gap as above | Directly verified (aggregate); handler-bypass directly verified | Flag for Phase 7: clarify whether the API even exposes a separate "start" action or whether the aggregate's `Start` method is entirely vestigial |
| TrainingJob | Complete (Running→Completed, requires accuracy score) | Domain method exists; not confirmed this phase whether any handler calls it (only Start/Cancel/Evaluate handlers were read in full) | Unverified this phase | Unverified | Unverified | Same gap | Pattern-inferred | Follow-up candidate |
| TrainingJob | Cancel (any non-terminal→Cancelled) | Yes, via `CancelTrainingJobCommandHandler` → `_repository.CancelAsync` (raw SQL) | **No at runtime** — see headline finding; the domain guard (`Status.IsTerminal` check, non-empty-reason check) never executes | **No** — `TrainingJobCancelledEvent`/`TrainingJobCancelledIntegrationEvent` never raised on the live path (Phase 3 `gaps/orphaned-ingress.md` #4 already flagged the *event class* as unused; this phase adds *why*: the whole aggregate is bypassed, not just the publish call) | Unverified | Same gap | Directly verified **[NEW]** | The clearest example in this whole discovery of "looks fully guarded, isn't" — worth prioritizing for remediation over aggregates that are merely Create-only, because this one creates false confidence |
| TrainingJob | RecordEvaluation (Fail if terminal) | Yes, via `EvaluateTrainingJobCommandHandler` → `_repository.RecordEvaluationAsync` (raw SQL) | **No at runtime** — same bypass pattern | Unverified whether `TrainingJobEvaluatedEvent` is raised on the live path (pattern strongly suggests no, consistent with Start/Cancel) | Unverified | Same gap | Directly verified (bypass); pattern-inferred (event) | |
| TrainingJob | Fail (any non-terminal→Failed) | Domain method exists; no handler found calling it this phase | Unverified | Unverified | Unverified | Same gap | Pattern-inferred | |
| TrainingJob | Delete | **No** — no such method anywhere | N/A | N/A | N/A | N/A | Directly verified | |

## AIInvestigations (Investigation)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Investigation | Open (Create, via `RunInvestigationCommandHandler`) | Yes | **No at runtime** — Phase 3 already confirmed `RunInvestigationCommandHandler` has no `IPublisher` reference; this phase additionally confirms (by the same grep pattern used for TrainingJob) that the handler does not construct an `Investigation` instance either — it calls the repository's create method directly, consistent with the systemic bypass pattern | No — confirmed no event of any kind (Phase 3, unchanged) | Unverified | Unverified this phase (Investigation's `TenantId` presence was not independently re-checked; flag for follow-up) | Directly verified (Phase 3 handler read + this phase's bypass-pattern grep) | |
| Investigation | AddFinding | Domain method exists (guarded — 2 `throw` sites found); no handler confirmed calling it this phase | Unverified | Unverified | Unverified | Unverified | Pattern-inferred | |
| Investigation | SubmitHypothesis | Domain method exists (guarded); no handler confirmed calling it this phase | Unverified | Unverified | Unverified | Unverified | Pattern-inferred | |
| Investigation | Cancel (via `CancelInvestigationCommandHandler`) | Yes, in the domain — **[NEW] confirmed bypassed**: handler calls `_repository.CancelAsync(...)` directly, never touching `Investigation.Cancel` | **No at runtime** — same bypass pattern as TrainingJob.Cancel; the domain guard (rejects if already terminal, requires non-empty cancellation reason) never executes | Unverified whether any event fires on the live SQL-only path (pattern suggests no) | Unverified | Unverified | Directly verified **[NEW]** | Second confirmed instance of the systemic aggregate-bypass pattern |
| Investigation | Complete | Domain method exists (guarded); no handler confirmed calling it this phase | Unverified | Unverified | Unverified | Unverified | Pattern-inferred | |
| Investigation | Delete | **No** — no such method | N/A | N/A | N/A | N/A | Directly verified | |

## Intelligence.DataLake subsystem (Intelligence's own ingestion surface, distinct from the top-level DataLake module)

| Concept | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| DataLakeRecord | Create (Ingest) | Yes | Input-validation only — no aggregate/state-machine class involved (Phase 3, unchanged) | No — `DataLakeIngestionCompletedIntegrationEvent` never published (Phase 3, unchanged) | Unverified | Optional `TenantId` on `IntelligenceControllerBase` (Phase 4, unchanged) | Directly verified (Phase 3 carried forward) | |
| Pipeline transformation | Transform | Yes | Input-validation only; delegates to `IPipelineOrchestrationService`, no local aggregate | **Yes** — the module's one genuine publish, via `IIntelligenceEventPublisher.PublishAsync` → Outbox (Phase 3, unchanged) | Unverified | Optional TenantId | Directly verified (Phase 3 carried forward) | Dead-ends at `NullOutboxPublisher` (Phase 3/4, unchanged) — the single most severe dead-end previously found in this discovery; this phase's TrainingJob/Investigation bypass finding is a *different*, arguably equally severe failure mode (guard logic that's dead code, vs. an event that's silently swallowed) |
| Any DataLake-subsystem concept | Update/Delete/Cancel | Not found this phase — no aggregate exists to have these transitions | N/A | N/A | N/A | N/A | Pattern-inferred (not exhaustively re-searched) | |

## Summary for this document

Intelligence presents a **different and arguably more insidious** version of the asymmetric-coverage
pattern the task asked about. It is not "Create is guarded, Delete isn't" — it is **"the domain
model appears to have a complete, well-guarded lifecycle for both `TrainingJob` and `Investigation`,
and none of it is reachable from any HTTP request."** A code reviewer reading only
`TrainingJob.cs`/`Investigation.cs` would conclude this module has the best-designed lifecycle
enforcement in the entire solution; a reviewer reading only the handlers would see plain
repository-passthrough CRUD with no domain logic at all. Both are true simultaneously, because the
two layers never connect. This should be treated as a **higher remediation priority than the
Create-only aggregates found in PlantSafety and DataLake**, precisely because it creates false
confidence — anyone who audited only the domain layer (as a quick code review often would) would
sign off on lifecycle safety that does not exist at runtime.
