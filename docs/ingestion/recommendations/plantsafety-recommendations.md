# Modernization Recommendations — PlantSafety

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Synthesizes Phases 1-8 (see citations per item). PlantSafety is 82% of the solution's API surface
(722/879 endpoints, 362 controllers, ~100 aggregates, 524 stored procedures) and carries the
solution's most severe *and* most fixable findings — most fixes below are "wire up something that
already exists correctly," not new design work.

---

## Top 5 fixes, in priority order

1. **Register the 147 feature-level authorization policies (or route through the already-written
   `AddPlantSafetyPolicies()`).** Fixes an active, confirmed, production-breaking defect: ~293+
   endpoints 500 on every call today. Effort: S. Risk of not fixing: **Availability** (confirmed,
   not theoretical — this is happening on every request to the affected routes right now).
2. **Register PlantSafety's 22 MassTransit consumers via `AddConsumer<T>()`, AND fix
   `OutboxProcessor` to publish the deserialized strongly-typed event instead of the raw
   `OutboxEnvelope`.** Both parts are required — registering consumers alone does not fix delivery
   because the published CLR type doesn't match what any consumer expects. Effort: M. Risk of not
   fixing: **Availability/tech-debt** (silent no-op, not a crash) today, but blocks 22 safety
   domains' entire event-driven design from ever working.
3. **Route all 58 write-repository `outboxWriter.Enqueue(...)` calls through the existing
   `IUnitOfWork.EnqueueEvent(domainEvent, tenantId, actorId, correlationId)` method** instead of
   calling `IOutboxWriter.Enqueue` directly with hardcoded `"system"`/`Guid.Empty` literals. The
   correct plumbing already exists, compiles, and is registered — it simply has zero callers.
   Effort: M (mechanical, ~58 call sites, but must be done carefully — see fix detail). Risk of not
   fixing: **Data integrity** (every audit trail / "who did this" question is permanently
   unanswerable for every mutating action PlantSafety has ever recorded).
4. **Wire the webhook receiver's "async outbox processing"** (the one genuine external-ingress
   point) so `ProcessInboundWebhookCommandHandler` actually enqueues to the outbox / raises a
   domain event after recording the raw payload, matching what its own header comment already
   promises. Effort: M. Risk of not fixing: **Data integrity / documentation-mismatch** — an
   external system is told its data was accepted and nothing is ever done with it.
5. **Add a `TriggerAlertCommandValidator`** with `MaximumLength` rules matching the SQL column
   sizes (`Title` ≤200, `LocationZoneName` ≤200), and audit the rest of the ~55 aggregates'
   commands for the same "validator absent/looser than the `AddString` size" pattern found here.
   Effort: S for Alerts specifically, M for the solution-wide audit. Risk of not fixing: **Data
   integrity** (silent ADO.NET truncation, no exception, row written with corrupted/shortened data).

---

## Security / Availability

### 1. Unregistered authorization policies — ~293+ endpoints 500 on every call
- **Problem**: `AddPlantSafetyAuthorization()` (`PlantSafety.Api\Extensions\ServiceCollectionExtensions.cs:117-135`)
  registers exactly two policies (`RequireAuthenticatedUser`, `RequireDashboardRead`). 293
  controllers use `[Authorize(Policy = "<Feature>AuthorizationPolicies.Read")]`-style names across
  147 feature-level policy classes; ~13 more use quoted module-level strings matching
  `AddPlantSafetyPolicies()` (`AuthorizationPolicyExtensions.cs:49-101`), which is **never invoked**
  anywhere in the solution. Every one of these 306 endpoints throws
  `InvalidOperationException: The AuthorizationPolicy named '...' was not found` inside ASP.NET
  Core's authorization middleware, caught by `ExceptionHandlingMiddleware` and converted to a
  generic RFC 7807 500 — fails closed, confirmed by tracing the middleware pipeline and by a forced
  clean build. (`security/rls-audit.md` §1, `gaps/tenant-violations.md` §1a.)
- **Fix**: Pick one canonical registration path and finish wiring it — either (a) call
  `AddPlantSafetyPolicies()` from `AddPlantSafetyAuthorization()` and reconcile its ~30
  module-level policy names against the 147 feature-level `<Feature>AuthorizationPolicies` class
  names (they currently use different naming conventions and don't match each other), or (b)
  generate/register the real 147 feature-level policy names directly in `AddAuthorization(...)`.
  Do **not** "fix" this by stripping the `Policy=` argument down to bare `[Authorize]` — per
  `rls-audit.md` §1g, that would convert a confirmed availability bug into a live cross-tenant data
  leak on any endpoint whose query still derives `TenantId` from `request.TenantId` rather than
  `env.TenantId`. Verify with an integration test that hits a sample of the 147 policy names and
  asserts 200/403, not 500.
- **Effort**: S-M (the registration itself is small; reconciling 147 policy names against ~30
  module-level names to decide which is canonical is the real work).
- **Risk of not fixing**: **Availability** — confirmed, not hypothetical. This is presently
  causing a large fraction of PlantSafety's read surface (its largest single population of
  endpoints) to be completely non-functional.

### 2. `PlantSafety.Api\Controllers\V1\PlatformV1Controller`-style bare `[Authorize]` landmine
- **Problem**: The legacy `BaseApiController` path has no actor-capture accessor at all (its own
  header comment lists this as a scoped-but-never-built "Future Extension Seam"). Currently low
  risk because the only two live controllers on this path are read-only/anonymous, but any future
  controller added under this base silently inherits the gap with no compiler/review signal.
  (`gaps/tenant-violations.md` §2.)
- **Fix**: Either retire `BaseApiController` in favor of the `RequestEnvelopeFactory` pattern for
  any new controller, or add a `CallerUserId` accessor to it now, before it's needed under time
  pressure.
- **Effort**: S.
- **Risk of not fixing**: **None today** (tech debt / landmine) — becomes a real actor-capture gap
  the moment a mutating endpoint is added to this base.

---

## Data Integrity

### 3. Every write-repository hardcodes `ActorId="system"`/`CorrelationId=Guid.Empty` on outbox writes
- **Problem**: All 58 of PlantSafety's `*DapperRepository.AddAsync` outbox-enqueue call sites
  (e.g. `AccidentDapperRepository.cs:124`) call
  `outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString())`
  — discarding the real `ActorId`/`CorrelationId` the command handler received via the
  `RequestEnvelopeFactory` envelope, even on the ~61 controllers using that pattern correctly up to
  the command object. The correct method, `IUnitOfWork.EnqueueEvent(IDomainEvent, string tenantId,
  string actorId, string correlationId)`, exists, is correctly implemented (`UnitOfWork.cs:40-41`
  passes parameters straight through), and has **zero callers** anywhere in the solution.
  (`security/actor-propagation.md` §1-2, `shape-audit/plantsafety-shape.md` Top-5 #1.)
- **Fix**: Change all 58 call sites from `outboxWriter.Enqueue(domainEvent,
  aggregate.TenantId.ToString(), "system", Guid.Empty.ToString())` to route through
  `unitOfWork.EnqueueEvent(domainEvent, aggregate.TenantId.ToString(), actorId, correlationId)`,
  threading the real `ActorId`/`CorrelationId` already available on the command
  (`CommandEnvelope.ActorId`/`CorrelationId`) through the handler → repository call chain. This is
  mechanical and low-risk — the plumbing already compiles and is registered; it's purely a matter
  of changing which method 58 nearly-identical call sites invoke and adding two parameters to the
  repository method signatures that currently only accept the aggregate. Also add
  `TenantId`/consistent `Actor` fields to the ~22 domain event contracts found missing them
  (`CultureCreatedEvent` and siblings) while doing this pass, since the same commit is the natural
  place to close both gaps together.
- **Effort**: M (58 call sites, mechanical pattern, but must verify each repository's method
  signature threads the values from the command handler correctly — not a pure find/replace).
- **Risk of not fixing**: **Data integrity** — every audit trail, compliance investigation, or
  "who did this" query against `OutboxMessages` is unrecoverable for literally every mutating
  action PlantSafety has ever recorded, including its own most-cited "working" example
  (`AccidentDapperRepository`).

### 4. Silent ADO.NET truncation via `StoredProcedureBuilder.AddString(name, value, size)`
- **Problem**: The shared helper (`PlantSafety.Infrastructure\Persistence\Dapper\SqlBuilder\StoredProcedureBuilder.cs:48-52`)
  sets an explicit `Size` on the SQL parameter smaller than the value's actual length, which causes
  `Microsoft.Data.SqlClient` to **silently truncate** the outgoing string before it reaches SQL
  Server — no exception, no error, row written with corrupted data. Confirmed concretely exploitable
  for `TriggerAlertCommand` (no validator exists at all for `Title`/`LocationZoneName`, both capped
  at 200 chars in SQL). This pattern is used by every one of the ~55 aggregate repositories; whether
  each one's FluentValidation cap is tighter than the SQL column (safe, as with AccidentManagement)
  or absent/looser (unsafe, as with Alerts) was not verified for all ~55.
  (`shape-audit/plantsafety-shape.md` Top-5 #2-3.)
- **Fix**: (a) Add `TriggerAlertCommandValidator` with `MaximumLength(200)` on `Title` and
  `LocationZoneName` immediately. (b) Run a solution-wide audit: for every `AddString(name, value,
  size)` call, confirm a FluentValidation `MaximumLength` exists on the corresponding command field
  and is ≤ the SQL size. (c) Consider a Roslyn analyzer rule (this repo already has an
  analyzer-rule-generation skill/infra) that flags any `AddString` call whose bound command field
  has no matching validator rule, to prevent regression.
- **Effort**: S (Alerts fix) / M (full audit) / M (analyzer rule, optional).
- **Risk of not fixing**: **Data integrity** — rows are silently written with truncated data and
  the API reports success; no error signal exists anywhere in the stack.

### 5. `ConcurrencyToken`/`RowVersion` columns are dead — zero real optimistic-concurrency protection
- **Problem**: `Accident.ConcurrencyToken` (`UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()`,
  regenerated on every update) and `Alert.RowVersion` (SQL Server `ROWVERSION`) exist in the schema
  but have no matching domain property, are never read back, and are never compared before a write.
  Two concurrent updates to the same row silently last-write-wins. (`shape-audit/plantsafety-shape.md`
  Top-5 #4.)
- **Fix**: Either wire these columns into the read/write cycle properly (read the token on
  fetch, pass it back on update, have the stored procedure reject a mismatched token) or remove
  them from the schema if optimistic concurrency isn't actually needed for this data — a
  half-built control that looks like protection but isn't is worse than no control, because it
  creates false confidence in code review.
- **Effort**: M per aggregate (there appear to be at least 2 confirmed instances, likely more
  pattern-inferred across the ~55 write repositories).
- **Risk of not fixing**: **Data integrity** (race condition on concurrent updates — likely
  low-frequency in practice given typical single-writer-per-record safety workflows, but a real
  silent-overwrite risk when it does occur).

---

## Event Wiring

### 6. 22 `*CreatedConsumer` classes never registered, and the type published doesn't match anyway
- **Problem** (restated from Top-5 #2 with full detail): `PlantSafety.Infrastructure.csproj`
  references real `MassTransit`/`MassTransit.RabbitMQ` 8.4.1, and
  `InfrastructureServiceRegistration.cs:222` calls real `AddMassTransit(x => x.UsingRabbitMq(...))`.
  But `x.AddConsumer<T>()` is never called for any of the 22 `*CreatedConsumer` classes — a live,
  connected RabbitMQ bus with zero bound receive endpoints. Separately,
  `OutboxProcessor.cs:21` calls `bus.Publish(envelope, ...)` where `envelope` is the raw
  `OutboxEnvelope` row object, not the strongly-typed domain event (`CultureCreatedEvent`, etc.)
  each consumer declares as its `IConsumer<T>` type parameter — so even after registering the
  consumers, the published type would never match. (`broker-ingress.md`, `gaps/missing-events.md`
  §1.2, `module-flow/plantsafety.md`.)
- **Fix**: Two changes, both required:
  1. In `InfrastructureServiceRegistration.cs`, inside the `AddMassTransit(x => ...)` block, add
     `x.AddConsumer<CultureCreatedConsumer>();` (and the other 21) alongside the existing
     `UsingRabbitMq(...)` call, with matching receive-endpoint configuration for each.
  2. In `OutboxProcessor.cs`, deserialize `envelope.Payload` back into its concrete event type
     (`Type.GetType(envelope.EventType)` or an event-type registry) before calling
     `bus.Publish(...)`, so the published CLR type matches what each `IConsumer<T>` expects.
  Then implement the actual side-effect logic each consumer's `// TODO: execute ... side-effects
  here` comment describes (currently all 22 are idempotency-check-then-log stubs) — registering
  and type-matching alone makes them *reachable*, not *useful*.
- **Effort**: M (registration + type-fix) + L (implementing 22 real side-effects, if that's in
  scope — flagged separately since it's a larger product decision, not just plumbing).
- **Risk of not fixing**: **Availability/tech-debt today** (silently does nothing, no error) —
  but this blocks the entire event-driven design for 22 safety domains (Culture, Compliance,
  OperationalSafety, SafetyEvents, Training) from ever functioning, and the 130+ Analytics read
  endpoints querying projections these consumers are meant to populate will keep returning empty/
  stale data indefinitely.

### 7. `AccidentCreatedEvent` and its 6 investigation-lifecycle siblings have no consumer of any kind
- **Problem**: Unlike the 22 `*CreatedEvent` pairs, `AccidentCreatedEvent`,
  `AccidentInvestigatorAssignedEvent`, `AccidentRootCauseRecordedEvent`,
  `AccidentCorrectiveActionCreatedEvent`, `AccidentCorrectiveActionCompletedEvent`,
  `AccidentRegulatoryFilingCompletedEvent`, `AccidentInvestigationClosedEvent` are raised and
  written to the outbox but have **no consumer class at all** — not even a stub. This is the single
  worst dead-end found in PlantSafety's event surface: AccidentManagement is the module's flagship,
  best-guarded aggregate (real cross-field business rules like the OSHA-filing check on Close), and
  its entire event-driven side is completely absent. (`gaps/orphaned-ingress.md` #1,
  `module-flow/plantsafety.md`.)
- **Fix**: Create at minimum a stub `AccidentCreatedConsumer` (and the 6 lifecycle-sibling
  consumers) matching the existing 22's shape, register them via `AddConsumer<T>()`, and treat as
  candidates for real implementation (regulatory-filing notifications, downstream investigation
  tracking) given how much domain-rule investment already exists in `Accident.cs`.
- **Effort**: S (stubs) / L (real implementation).
- **Risk of not fixing**: **Availability/tech-debt** — no data loss (the row is written), but zero
  observability or downstream reaction to the module's most important workflow.

### 8. All non-Created lifecycle events (~30+ types across ~26 aggregates) have zero consumers, not even stubs
- **Problem**: Every `Updated`/`Closed`/`Cancelled`/`Completed`/`Assigned`/`Recorded`-style domain
  event raised by the rich aggregates (Alert, Risk, SafetyPolicy, PermitWorkflow, Scorecard,
  ChecklistTemplate/Completion, AuditEngagement/AuditFinding, CultureAssessment/StopWorkEvent,
  PMScheduling/WorkOrderPriority) is enqueued to the outbox and then goes nowhere — confirmed by
  grep across 26 aggregate files, zero consumer classes matching those event-name suffixes exist.
  Only the 22 `Created` events got even a no-op stub. (`gaps/orphaned-ingress.md` #4,
  `events/publishing-map.md`.)
- **Fix**: Same shape as #6/#7 — this is a large population, so prioritize by business value
  (e.g. `AccidentInvestigationClosedEvent` and `RiskCreatedEvent`/`Close` transitions likely matter
  more than PPE inventory events for cross-cutting notification value). Don't build 30+ consumers
  speculatively; build them alongside whatever real downstream reaction (notification, projection
  update, compliance alert) is actually needed next.
- **Effort**: L (broad scope; recommend scoping to 3-5 highest-value aggregates first, not all 26
  at once).
- **Risk of not fixing**: **Tech-debt/availability** — no data loss, but the module's entire
  non-Create lifecycle surface (the majority of its write activity by method count) produces zero
  observable downstream signal.

### 9. Webhook receiver's documented "async outbox processing" doesn't exist
- **Problem**: `WebhookReceiverV1Controller`'s own header comment claims fast-ack-then-async-outbox
  processing; the actual handler (`ProcessInboundWebhookCommandHandler`) only calls
  `IInboundWebhookRepository.RecordEventAsync` to store the raw payload — no outbox enqueue, no
  domain event, no parsing/routing after the write. This is the one point where an *external
  system* is told its data was accepted and nothing happens to it afterward.
  (`gaps/orphaned-ingress.md` #5, `module-flow/plantsafety.md` §Webhooks.)
- **Fix**: Either implement the documented behavior (enqueue an `InboundWebhookReceivedEvent` to
  the outbox after the raw-payload write, with a consumer that parses/routes by `EventType`/
  `Source`), or update the header comment to match reality if async processing genuinely isn't
  built yet and isn't imminent — don't leave documentation and code contradicting each other on
  the solution's only real external-ingress point.
- **Effort**: M.
- **Risk of not fixing**: **Data integrity / operational** — external partners believe their
  webhook payloads are being processed; today they are only ever stored, never acted on.

---

## Lifecycle Gaps

### 10. ~50 aggregates never grew past Create — no Update/Delete/Cancel exists in the domain model
- **Problem**: A majority of PlantSafety's ~100 aggregates (Culture, Auditing[marker],
  Certification, ManagementOfChange, Permits[Compliance], Policy, Procedures, RegulatoryCalendar,
  PPEManagement[Operational], RiskManagement[Operational], Schedules[Operational],
  Training[Operational], EmergencyManagement, IncidentManagement, Observation, Dashboard,
  Engagement, PredictiveAnalytics, ScoreCards, and ~12 more) have **no** Update/Delete/Cancel/
  status-transition method anywhere in the domain model — Create is their entire public surface.
  This is exactly the set behind the 22 no-op-consumer pairs. (`lifecycle-coverage/plantsafety-lifecycle.md`
  structural finding #2.)
- **Fix**: This is a product/scope decision, not a bug to patch reflexively — some of these may be
  intentionally simple lookup/reference-style records. Recommend a per-aggregate review with
  product owners to classify each as (a) genuinely append-only by design (leave as-is), or (b)
  missing lifecycle that should be built. Do not add Update/Delete/Cancel to all ~50 speculatively.
- **Effort**: L (review) + variable per aggregate that needs real lifecycle work.
- **Risk of not fixing**: **None-just-tech-debt** for the aggregates that are genuinely
  append-only by design; **availability/functional gap** for any that actually need correction
  workflows users are currently unable to perform (this needs the product review to know which).

### 11. Duplicate/parallel aggregate implementations for the same concept — only one is live
- **Problem**: Three confirmed instances of two (in one case three) independently-SQL-backed
  representations of the same business concept coexisting: (a) **Alerts** —
  `PlantSafety.Domain.Alerts.Alert.Aggregates.Alert` (live, wired) vs
  `PlantSafety.Domain.Operational.Alerts.Aggregates.Alert` (larger "canonical reference" shape) vs
  the read-only `OperationalSafety.Alerts` projection — three different tables, different column
  names, different lifecycle enums; (b) **RiskManagement** — the live, rich top-level `Risk` vs a
  Create-only `Operational.RiskManagement.RiskManagement` marker whose own controller is GET-only;
  (c) **Auditing** — the live, rich `AuditEngagement`/`AuditFinding` vs the Create-only
  `Compliance.Auditing.Auditing` marker (one of the 22 no-op-consumer pairs).
  (`shape-audit/plantsafety-shape.md` §Alerts, `lifecycle-coverage/plantsafety-lifecycle.md`
  structural finding #3.)
- **Fix**: For each pair/triple, confirm definitively which is live (controller wiring already
  points at this — Risk and Auditing are confirmed; Alerts' third leg needs the same
  confirmation), then formally deprecate and remove the dead duplicate(s) — don't leave two
  "Risk" or "Alert" concepts sitting in the same solution for a future engineer to accidentally
  wire the wrong one into a new feature. This is exactly the kind of drift the architecture-
  enforcement tooling already in this repo (`architecture-enforcement-skill`) should be pointed at.
- **Effort**: M per concept (mostly deletion + a migration/decommission plan, since SQL tables
  are involved).
- **Risk of not fixing**: **None-just-tech-debt** today, but a live landmine for future
  development — a new engineer extending "Risk" or "Alert" functionality has a 50/50 chance of
  picking the dead duplicate, and the resulting work would silently not reach any live controller.

### 12. The one correctly-built `Cancel` transition in the entire domain layer is unreachable from any API endpoint
- **Problem**: `PMScheduling.Cancel(string reason, ...)` is the **only** `Cancel(...)` method
  found across all ~100 aggregates, and it's genuinely well-built (status guard, non-empty-reason
  check, raises `PMSchedulingCancelledEvent`, has the AuditLog insert wired). But
  `SchedulingV1Controller.cs` (full read, every route enumerated) exposes `schedule`,
  `record-completion`, `escalate` — **no `cancel` route exists.** (`lifecycle-coverage/plantsafety-lifecycle.md`
  structural finding #4.)
- **Fix**: Add `POST api/v{version}/scheduling/pmscheduling/{id}/cancel` to
  `SchedulingV1Controller`, wired to the existing, already-correct domain method and command
  infrastructure — this is close to a pure "add the missing controller action" task since the
  domain/command layer is already done.
- **Effort**: S.
- **Risk of not fixing**: **Availability/operational** — equipment-decommissioning/PM-cancellation
  can never be recorded through the API today, despite the backend fully supporting it.

### 13. "Archive" transitions never publish an event (recurring pattern across rich aggregates)
- **Problem**: `Accident.Archive`, `Risk.Archive`, and `Scorecard.Archive` are all confirmed
  guarded (status check present) but never call `AddDomainEvent` — an archival action leaves no
  audit-trail signal downstream. `SafetyPolicy.Archive`/`PermitWorkflow.Archive` are suspected to
  share the pattern (guard confirmed, event-publish status unverified). This reads as a deliberate
  design choice (Archive is meant to be a silent terminal bookkeeping step) rather than an
  oversight, given how consistent it is across independently-implemented aggregates.
  (`lifecycle-coverage/plantsafety-lifecycle.md` cross-cutting notes.)
- **Fix**: Confirm with product/compliance stakeholders whether "record left active retention"
  needs to be observable downstream (e.g. for compliance-retention tooling). If yes, add the
  missing `AddDomainEvent` call to each Archive method (small, mechanical). If the silent-terminal
  design is intentional, document it explicitly in each aggregate's header comment so it isn't
  mistaken for a bug in a future audit.
- **Effort**: S per aggregate if event-publish is wanted.
- **Risk of not fixing**: **None-just-tech-debt** if genuinely intentional; **data-integrity/
  compliance gap** if retention-tracking tooling is ever built expecting this signal to exist.

---

## Deliberately not recommended

- **Do not build a general "fix all 22 consumers to do real work" project as a single Phase-9
  action.** That is a scope decision belonging to whoever owns each of the 22 safety domains
  (Culture, Compliance, etc.) — this document recommends *wiring* (registration + type fix) as the
  prerequisite, and flags implementation as a separately-scoped follow-up per domain, not a
  monolithic rewrite.
- **Do not add Update/Delete/Cancel to all ~50 Create-only aggregates speculatively** (#10) — many
  may be intentionally simple by design; a product review should decide which, not a blanket
  engineering pass.
