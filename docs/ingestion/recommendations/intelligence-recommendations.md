# Modernization Recommendations — Intelligence

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Intelligence's defining characteristic is a split personality: `TrainingJob` and `Investigation`
are genuinely well-designed domain aggregates with real state-machine guards — and neither is ever
invoked by the running application. Combined with a `NullOutboxPublisher` silently swallowing the
module's one working publish pipeline and zero authentication, this is the module where "looks
correct on a domain-layer code review" and "does nothing at runtime" diverge furthest.

---

## Top 5 fixes, in priority order

1. **Replace `NullOutboxPublisher` with a real `IOutboxPublisher` implementation** (Service Bus,
   RabbitMQ, or whatever transport the team intends) — this is the module's one flow that
   correctly writes to the Outbox and gets silently marked "delivered" while never actually being
   sent anywhere. Effort: M (assuming a transport choice is already made elsewhere in the
   solution; L if not). Risk of not fixing: **Data integrity** (messages are recorded as
   successfully delivered when they were not — this actively hides a production gap from
   monitoring).
2. **Fix `StartTrainingJobCommandHandler`/`CancelTrainingJobCommandHandler`/
   `EvaluateTrainingJobCommandHandler` (and the AIInvestigations equivalents) to construct/load
   and call methods on the `TrainingJob`/`Investigation` aggregates**, instead of calling the
   Dapper repository directly against a stored procedure. Effort: L (requires rearchitecting each
   handler's persistence flow to route through the aggregate, plus confirming/adding the matching
   guard to the underlying stored procedures). Risk of not fixing: **Data integrity** (state
   transitions — cancel, complete, fail — currently have zero enforcement anywhere in the stack if
   the stored procedures also lack a guard, which could not be confirmed from source).
3. **Wire authentication middleware (JWT bearer + `[Authorize]`) into `Intelligence.Api`.**
   Effort: M. Risk of not fixing: **Security** (confirmed — zero authentication anywhere in the
   module, the same gap as DataLake).
4. **Add `TenantId` as a first-class column on `Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage`**
   (and an `ActorId` column), rather than leaving both buried (if present at all) in the opaque
   `Payload` JSON blob. Effort: S-M. Risk of not fixing: **Data integrity** (cross-tenant mixing
   risk in any future dead-letter/retry tooling; also compounds with finding #3 in the shape audit
   — `Investigation`'s own actor field is already narrowed from `UserId` to bare `string` before it
   even reaches the outbox).
5. **Instantiate/publish the never-raised events** (`DataLakeIngestionCompletedIntegrationEvent`,
   `TrainingJobStartedIntegrationEvent`, `TrainingJobCancelledIntegrationEvent`) from their
   respective handlers, or delete them if superseded by the aggregate-level events once #2 lands.
   Effort: S-M. Risk of not fixing: **None-just-tech-debt** today, but should be resolved as part
   of the same work as #2 rather than left as a second, parallel unused event surface.

---

## Security / Availability

### 1. Zero authentication middleware wired
- **Problem**: `IntelligenceControllerBase.RequestingUserId` is a required, non-nullable accessor
  and `TenantId` is optional (`Guid?`) — but zero `AddAuthentication`/`AddJwtBearer`/`[Authorize]`
  exist anywhere in `Intelligence.Api`. An in-process `AuthorizationBehavior` evaluates
  business-rule authorization for requests implementing `IHasAuthorization`, but it has nothing
  trustworthy to authorize against, since no HTTP-layer authentication establishes who the caller
  claims to be in the first place. (`classification-matrix.md` §Intelligence,
  `security/rls-audit.md` §3.)
- **Fix**: Same pattern as DataLake — add `AddAuthentication(...).AddJwtBearer(...)` and
  `[Authorize]` (or a fallback policy) to `Intelligence.Api`'s DI setup, mirroring Azure's
  already-correct implementation.
- **Effort**: M.
- **Risk of not fixing**: **Security** — confirmed gap; `AuthorizationBehavior`'s business-rule
  checks are running against an unauthenticated identity today.

---

## Data Integrity

### 2. `NullOutboxPublisher` is the only registered `IOutboxPublisher` — silently swallows the one working publish pipeline
- **Problem**: `TransformDataCommandHandler` correctly serializes `DataLakeTransformationCompletedIntegrationEvent`
  and writes it to the Outbox via `IIntelligenceEventPublisher.PublishAsync` — this part is genuinely
  well-engineered, durable write-then-relay. `OutboxProcessor` (a real, running `BackgroundService`)
  polls this table and calls `IOutboxPublisher.PublishAsync` per row. The only registered
  implementation, `NullOutboxPublisher`, logs a warning ("no event bus implementation has been
  wired up") and returns `Task.CompletedTask`. `OutboxProcessor` then marks the row `Processed =
  true` — the event is durably recorded as delivered while never having gone anywhere. This is the
  single most severe dead-end found across the entire solution-wide discovery: not merely "nothing
  consumes this," but an active stand-in that lies about success. (`module-flow/intelligence.md`
  Flow 3, `events/publishing-map.md`.)
- **Fix**: Implement and register a real `IOutboxPublisher` (Service Bus, RabbitMQ, or whatever
  transport is canonical elsewhere in the solution — note PlantSafety already has a working
  MassTransit+RabbitMQ setup that could inform the choice, though registration is not itself
  sufficient there either, see the PlantSafety recommendations doc) via
  `services.AddSingleton<IOutboxPublisher, YourRealPublisher>()`, replacing the
  `NullOutboxPublisher` registration in `Intelligence.Infrastructure.DependencyInjection.cs:136`.
  At minimum, until a real transport is chosen, change `NullOutboxPublisher` to **not** mark the
  message `Processed = true` on failure-to-deliver, so the dead-end is visible in the outbox table
  (unprocessed backlog) rather than hidden.
- **Effort**: M (assuming transport infra exists elsewhere to reuse) to L (if building from
  scratch).
- **Risk of not fixing**: **Data integrity** — this is an active monitoring/observability hazard,
  not just a missing feature: anyone checking "is the outbox healthy" today would see 100%
  processed, 0% backlog, and conclude everything is fine.

### 3. `TrainingJob`/`Investigation` aggregates are fully guarded — and completely bypassed by their own handlers
- **Problem**: `TrainingJob.cs` and `Investigation.cs` implement genuinely correct state
  machines (`Submit`/`RegisterDataset`/`Start`/`Complete`/`Cancel`/`RecordEvaluation`/`Fail` for
  TrainingJob; `Open`/`AddFinding`/`SubmitHypothesis`/`Cancel`/`Complete` for Investigation), each
  with real cross-field invariants (e.g. `Start` requires Pending **and** a registered dataset) and
  correctly-shaped domain events. But every handler that should invoke these
  (`StartTrainingJobCommandHandler`, `CancelTrainingJobCommandHandler`,
  `EvaluateTrainingJobCommandHandler`, `CancelInvestigationCommandHandler`) calls the Dapper
  repository directly against a raw stored procedure, never constructing or loading the aggregate.
  The guard clauses never execute at runtime. Whether the underlying stored procedures
  (`intelligence.usp_TrainingJob_Cancel`, etc.) have an equivalent `WHERE Status NOT IN (...)`
  guard could not be confirmed — Intelligence has no source-controlled SQL (unlike PlantSafety).
  This is flagged as a **higher remediation priority than the Create-only aggregates found in
  PlantSafety/DataLake**, because it creates false confidence: a code reviewer reading only the
  domain layer would sign off on lifecycle safety that does not exist at runtime.
  (`lifecycle-coverage/intelligence-lifecycle.md` headline finding.)
- **Fix**: Rearchitect the four confirmed-bypassed handlers (and any others following the same
  pattern — `RegisterDataset`, `Complete`, `Fail` on `TrainingJob`, and `AddFinding`/
  `SubmitHypothesis`/`Complete` on `Investigation` were not confirmed to have callers at all this
  phase and may be fully dead code) to load the aggregate via its repository, invoke the guarded
  domain method, and persist the result — the standard CQRS pattern this codebase uses correctly
  elsewhere (PlantSafety's rich aggregates follow this shape). As a prerequisite, locate or
  reconstruct the stored procedures' actual bodies to confirm whether they independently enforce
  any state guard today, since that determines whether this is "redundant safety net missing" or
  "zero enforcement exists anywhere."
- **Effort**: L (touches the persistence architecture for both aggregates, not a small patch).
- **Risk of not fixing**: **Data integrity** — training jobs and investigations can potentially be
  cancelled/completed/failed in any order with zero enforcement, if the SQL layer doesn't
  independently guard it (unconfirmed either way from source).

### 4. Intelligence's Outbox and domain aggregates both drop tenant/actor type fidelity
- **Problem**: Two independent, compounding gaps: (a) `TrainingJob` the aggregate has **no
  `TenantId` property at all** (confirmed by full read of its property list); (b)
  `Investigation.RequestedByActorId` narrows from the strongly-typed `UserId` (as validated at the
  command boundary) to a plain `string` by the time it reaches the aggregate constructor — a
  type-safety narrowing in the wrong direction; (c) `Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage`
  has neither a `TenantId` nor `ActorId` column, unlike DataLake's `OutboxMessage` which does carry
  `TenantId` correctly. By the time an Intelligence domain event would reach a downstream consumer,
  actor/tenant identity has been stripped of type fidelity twice and may not survive as a queryable
  column at all. (`shape-audit/intelligence-shape.md` finding #2, `gaps/tenant-violations.md` §4.)
- **Fix**: Add `TenantId` to the `TrainingJob` aggregate itself (not just the command/repository
  layer); keep `RequestingUserId`/`RequestedByActorId` as a strongly-typed `UserId` all the way to
  the aggregate constructor rather than narrowing to `string`; add first-class `TenantId`/`ActorId`
  columns to `OutboxMessage`, populated from the aggregate/event at write time.
- **Effort**: M (touches aggregate constructors, command handlers, and the outbox schema
  together — do as one coordinated change, not three separate ones, since they're the same root
  cause).
- **Risk of not fixing**: **Data integrity** — cross-tenant mixing risk in any future dead-letter/
  retry tooling (currently moot only because the pipeline dead-ends at `NullOutboxPublisher`, item
  #2 — fixing #2 without also fixing this makes the gap live).

---

## Event Wiring

### 5. Several event classes defined, never raised
- **Problem**: `DataLakeIngestionCompletedIntegrationEvent` (Intelligence's own DataLake
  ingestion subsystem — distinct from the top-level DataLake module), `TrainingJobStartedIntegrationEvent`,
  `TrainingJobCancelledIntegrationEvent` are all defined but never referenced by any handler —
  confirmed via grep across `Intelligence.Application.AITraining`/`.DataLake`.
  (`module-flow/intelligence.md` Flow 2, §AITraining.)
- **Fix**: Raise these from their respective handlers once item #3 (aggregate bypass) is
  addressed — the natural place for these events to originate is the aggregate's own guarded
  transition methods (`TrainingJob.Submit`/`.Cancel` etc. already raise the *correctly-named*
  domain events; the Application-layer `*IntegrationEvent` variants may be redundant with those
  once the aggregate is actually invoked, or may serve a distinct cross-module integration purpose
  — clarify intent before building both).
- **Effort**: S-M, folded naturally into item #3's larger fix.
- **Risk of not fixing**: **None-just-tech-debt** until item #3 lands, at which point it becomes
  the natural next step.

---

## No lifecycle-gaps section for this module by design

Intelligence's asymmetric-coverage problem is not "some aggregates are Create-only" (PlantSafety's
and DataLake's pattern) — it is the aggregate-bypass pattern covered in Data Integrity item #3
above, which is the more severe and more specific version of the same underlying concern. No
additional Create-only-aggregate findings are called out separately for this module.

---

## Deliberately not recommended

- **Not recommending a full SQL-source-control effort for Intelligence in this document** (parallel
  to DataLake's recommendation) — flagging it here as a known gap (item #3's fix depends on it)
  but not elevating it to a Top-5 item on its own, since DataLake's version of the same gap is
  already covered in that module's document and the fix pattern would be identical.
