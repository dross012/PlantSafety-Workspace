# Module Flow — PlantSafety

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Scope and method

PlantSafety has 722 of the solution's 879 API endpoints (82%), spread across 362 controller files
and organized into large feature domains: AccidentManagement, Alerts, Analytics (Culture,
Dashboard, Engagement, Predictive, PredictiveAnalytics), Compliance (Auditing, Certification,
ManagementOfChange, Permits, Policy, Procedures), OperationalSafety (Alerts, Checklists,
PPEManagement, RiskManagement, Schedules), SafetyEvents (EmergencyManagement, IncidentManagement,
Observation, CrisisManagement, NearMisses, RootCauseAnalysis), Operational/Training
(CompetencyManagement), CrossDomain (shared request-context infrastructure), and Webhooks (the
sole external ingress).

Given the scale, this document traces **2-3 representative flows per feature area in full** (every
file in the chain read directly) and states explicitly, for every sibling not individually traced,
that the pattern is inferred from the shared `RequestEnvelopeFactory` / domain-event / outbox /
consumer scaffolding that Phase 2 confirmed is structurally identical across all 22
`*CreatedEvent`/`*CreatedConsumer` pairs and all `RequestEnvelopeFactory`-pattern controllers.

**Important correction to an established Phase 1/2 fact, found by direct evidence this phase:**
Phase 1/2 stated *"No MassTransit... usage found anywhere... IConsumer<T>/ConsumeContext<T> are a
custom in-process abstraction, not MassTransit's actual bus."* This is **incorrect** and is
corrected here:

- `PlantSafety.Infrastructure.csproj` (lines 203-204) references the **real** NuGet packages
  `MassTransit` (8.4.1) and `MassTransit.RabbitMQ` (8.4.1).
- `InfrastructureServiceRegistration.cs` (line 222) calls the **real** `services.AddMassTransit(x
  => x.UsingRabbitMq(...))`, configured against a RabbitMQ host (`localhost` by default, overridable
  via config) — this is genuine MassTransit bus wiring, not a look-alike.
- `IdempotentConsumerBase<TMessage>` genuinely implements MassTransit's real `IConsumer<TMessage>`
  and consumes a real `ConsumeContext<TMessage>`.
- **However**, `AddMassTransit(...)` registers **zero consumers** (`x.AddConsumer<T>()` is never
  called anywhere in the registration). This means: even though a real message bus is stood up and
  connected to RabbitMQ, **none of the 22 `*CreatedConsumer` classes are ever bound to a receive
  endpoint** — publishing a message via `IBus.Publish(...)` will not reach them, because MassTransit
  has no configured queue/endpoint to route it to. The net behavioral effect (nothing happens
  downstream) matches what Phase 1/2 observed, but the underlying cause is different and matters
  for Phase 4/7: this is "real broker, unregistered consumers," not "fake broker abstraction." See
  `PlantSafety\PlantSafety.Infrastructure\DependencyInjection\InfrastructureServiceRegistration.cs:203-241`,
  `PlantSafety\PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs`.
- `OutboxProcessor.cs` (`PlantSafety.Infrastructure.BackgroundJobs`) resolves `IBus` from DI and
  calls `bus.Publish(envelope, ...)` where `envelope` is the raw `OutboxEnvelope` row object (not
  the strongly-typed domain event) — so even the outbox's own drain step publishes a message type
  that has no registered consumer either.

## Feature area: AccidentManagement

**Traced flow — Report Accident (Create):**

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v{version}/accidentmanagement` | `AccidentManagementV1Controller.CreateAsync` → `CreateAccidentCommandHandler` | `Accident` aggregate (`PlantSafety.Domain.AccidentManagement.Aggregates.Accident`) | `Accident.Create(...)` sets `Status = AccidentStatus.Reported` | Dapper (`IAccidentRepository` → `AccidentDapperRepository`), stored procedure `[AccidentManagement].[usp_Accident_Insert]` | **`AccidentCreatedEvent`** — raised via `agg.AddDomainEvent(...)` inside `Accident.Create`, then enqueued to the Outbox by `AccidentDapperRepository.AddAsync` (`outboxWriter.Enqueue(domainEvent, ...)`) | **None.** `AccidentCreatedEvent` is **not** one of the 22 `*CreatedEvent`/`*CreatedConsumer` pairs Phase 1/2 catalogued — grepped the whole PlantSafety tree: it appears only in `Accident.cs` (raise site) and its own event-class definition file. There is no `AccidentCreatedConsumer` at all, not even a stub. | Directly verified (controller, handler, aggregate, repository all read in full; grep confirms zero consumer) |

**Dead-end, most severe variant found in PlantSafety**: this event is written to the outbox
(so it will be picked up by `OutboxProcessor` and pushed to `IBus.Publish`), but there is no
consumer class of any kind — not even a no-op stub — subscribing to `AccidentCreatedEvent`.
Compare to the 22 `*CreatedEvent` pairs below, which at least have a (non-functional) consumer.

The other 6 AccidentManagement lifecycle commands on the same controller
(`AssignInvestigator`, `RecordRootCause`, `CreateCorrectiveAction`, `CompleteCorrectiveAction`,
`FileRegulatoryReport`, `CloseInvestigation`) each raise their own domain event
(`AccidentInvestigatorAssignedEvent`, `AccidentRootCauseRecordedEvent`,
`AccidentCorrectiveActionCreatedEvent`, `AccidentCorrectiveActionCompletedEvent`,
`AccidentRegulatoryFilingCompletedEvent`, `AccidentInvestigationClosedEvent` — all confirmed
present in `Accident.cs` via grep) through the identical `AddDomainEvent` → outbox-enqueue
mechanism. None of these has a consumer class either (same grep scope). Pattern-inferred: same
dead-end as `AccidentCreatedEvent`.

## Feature area: OperationalSafety (Alerts)

**Traced flow — Trigger Alert:**

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v{version}/alerts` | `AlertsV1Controller.TriggerAsync` → `TriggerAlertCommandHandler` | `Alert` aggregate (`PlantSafety.Domain.Alerts.Alert.Aggregates.Alert`) | `Alert.Trigger(...)` sets initial `Status`/`Severity` | `IAlertCommandRepository` (registered in DI as `AlertsDapperRepository`) — Dapper/stored-procedure | `AlertCreatedEvent` (pattern-inferred to follow the same `AddDomainEvent`→outbox path as Accident, based on identical repository/aggregate shape — not independently re-verified for the enqueue call site) | **`AlertsCreatedConsumer`** (`PlantSafety.Infrastructure.OperationalSafety.Alerts.Messaging`) — a genuine MassTransit `IConsumer<AlertCreatedEvent>`, but **unregistered** (no `AddConsumer<AlertsCreatedConsumer>()` call found anywhere) and, even if it were registered and reached, its body is a **no-op stub**: idempotency check → one `ILogger.LogInformation` line → mark processed. No side effect of any kind. | Directly verified (controller, handler, and consumer all read in full) |

The remaining 7 Alert lifecycle mutations on the same controller (Acknowledge, Dispatch, Escalate,
Resolve, Close, RecordDelivery, RecordResponseAction) follow the identical envelope-command
pattern per the controller's own onboarding-notes header comment (directly read). Not
individually traced to their handlers/aggregate methods this phase — pattern-inferred.

## Feature area: Analytics / Culture (read-heavy pattern)

**Traced flow — Get Culture Behavior Analytics (read):**

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `GET api/v{version}/analytics/culture/behavior` | `BehaviorV1Controller.GetAsync` (direct `: ControllerBase`, not `RequestEnvelopeFactory`) → `GetBehaviorQuery` handler | Read-only projection (no aggregate mutated) | None — pure read | Dapper query repository (read-only) | None — reads never raise domain events | N/A | Directly verified in Phase 2 (`BehaviorV1Controller.cs` full read); re-confirmed present this phase via `api-ingress.md` route listing |

**Notable, carried over from Phase 2 and independently significant for a flow map**: this
controller builds `new GetBehaviorQuery(request.TenantId, ...)` directly from a client-supplied
query-string `TenantId`, not from the authenticated tenant-context claim used by the
envelope-pattern controllers. This is a *read path*, so it never touches the event/lifecycle
dimension this document otherwise tracks, but it means Analytics/Culture/Dashboard/Engagement/
PredictiveAnalytics's 130 GET-only endpoints (Behavior, CultureIndex, EngagementScoring,
TrendModeling ×2 versions each, Dashboard's Executive/KPISnapshots/ReadModel/TrendAnalysis ×2,
Engagement, PredictiveAnalytics, Predictive's ComplianceFailure/FatigueRiskModeling/
IncidentPrediction/RiskForecasting ×2) are **read/projection endpoints with no lifecycle,
repository-write, or event dimension at all** — they exist to serve the 5 domains'
`*CreatedConsumer` projections (Culture, Dashboard, Engagement, PredictiveAnalytics, ScoreCards)
that Phase 2 found are themselves still no-op stubs. In other words: **these 130+ analytics read
endpoints are querying projection tables that the corresponding consumers don't yet populate**,
per Phase 2's own finding that all 5 have an explicit `// TODO: execute ... side-effects here`
comment. Flagged for Phase 4 as a very concrete gap: reads exist, the write-side projection that
should feed them does not.

## Feature area: Compliance (Auditing / Certification / ManagementOfChange / Permits / Policy / Procedures)

This feature area contains 120+ controller files (V1/V2 pairs across ~10 sub-areas each), the
large majority of which are read-only V1/V2 analytics-style controllers matching the Culture
pattern above (pattern-inferred from directory listing + the Compliance/Auditing consumer already
directly verified in Phase 2).

**Traced (via Phase 2 full read, re-confirmed this phase): `AuditingCreatedConsumer`.**

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| (Auditing create command — not re-traced from controller this phase; consumer side confirmed) | `AuditingCreatedConsumer.Consume` | N/A (consumer side) | N/A | `System.ProcessedEvents` (idempotency ledger only — no domain write) | Consumes `AuditingCreatedEvent` | Itself is the terminal node — idempotency check, one log line, mark processed. **No projection, no notification, no side effect.** Same unregistered-MassTransit-consumer caveat as Alerts applies here too. | Directly verified (Phase 2 full read of this exact file, re-confirmed present this phase) |

The other 12 `*CreatedConsumer` classes under Compliance (`CertificationCreatedConsumer`,
`ComplianceCreatedConsumer`, `ManagementOfChangeCreatedConsumer`, `PermitsCreatedConsumer`,
`PolicyCreatedConsumer`, `ProceduresCreatedConsumer`, `RegulatoryCalendarCreatedConsumer`) share
the byte-for-byte identical structure (Phase 2 scripted check across all 22). Pattern-inferred.

## Feature area: SafetyEvents (EmergencyManagement / IncidentManagement / Observation)

Controllers confirmed present: `EmergencyManagementV1/V2Controller`,
`IncidentManagementV1/V2Controller` (+ a canonical `IncidentsCanonicalV1Controller`),
`ObservationV1/V2Controller`, plus read-only `CrisisManagement`, `DrillsAndExercises`,
`NearMisses`, `RootCauseAnalysis` controllers. `EmergencyManagementCreatedConsumer`,
`IncidentManagementCreatedConsumer`, `ObservationCreatedConsumer` are 3 of the 22 confirmed no-op
consumers (Phase 2 scripted check; not re-opened individually this phase — pattern-inferred from
the identical `Auditing`/`Culture` bodies directly verified).

## Feature area: Operational / Training (CompetencyManagement)

`CompetencyManagementV1/V2Controller` (under `PlantSafety.Api.Operational.CompetencyManagement`)
matches the same envelope-command / domain-event / outbox / `CompetencyManagementCreatedConsumer`
shape as the other 21 `*CreatedConsumer` pairs (Phase 2 confirmed no-op body). Not independently
re-traced from controller to aggregate this phase — pattern-inferred.

## Feature area: CrossDomain

`PlantSafety.Api.CrossDomain` (617 hits per Phase 1) is shared cross-cutting API infrastructure —
`TenantContextAccessor`, `CorrelationIdAccessor`, etc. — not a distinct feature domain with its own
controllers/aggregates/events. No ingress flow to trace here; it is infrastructure consumed by the
other feature areas' controllers.

## Feature area: Webhooks (the sole external ingress point)

**Traced flow — Inbound Webhook Receive:**

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/webhooks/receive` `[AllowAnonymous]`, HMAC-SHA256 verified | `WebhookReceiverV1Controller` → `ProcessInboundWebhookCommand` → `ProcessInboundWebhookCommandHandler` | No domain aggregate — raw inbound payload is recorded as-is | None — `IInboundWebhookRepository.RecordEventAsync` simply stores `EventId, EventType, Source, TenantId, SiteId, OccurredAt, RawPayload` | Repository call, presumed SQL (not independently opened this phase) | **None.** The handler does not enqueue to the outbox, does not raise a domain event, and does not dispatch further processing of any kind — despite the controller's own header comment claiming *"actual processing happens asynchronously via the outbox."* | N/A — nothing downstream | Directly verified (controller header + full handler read) |

**Dead-end, and a documentation/implementation mismatch worth flagging distinctly for Phase 4**:
the controller's own doc comments describe a fast-ack-then-async-outbox-processing design, but the
handler that actually runs only writes the raw payload to a table. There is no evidence anywhere
in the solution that a received webhook payload is ever parsed, routed, or acted upon after this
write — the "async outbox processing" the header comment promises does not exist in the traced
code path.

## Summary for Phase 4 — PlantSafety dead-ends, by severity

1. **Worst**: `AccidentCreatedEvent` (and its 6 investigation-lifecycle siblings) — raised, written
   to outbox, **zero consumer of any kind exists**, not even a stub.
2. **Bad**: The 22 `*CreatedEvent`/`*CreatedConsumer` pairs (Culture, Dashboard, Engagement,
   PredictiveAnalytics, ScoreCards, Auditing, Certification, Compliance, ManagementOfChange,
   Permits, Policy, Procedures, RegulatoryCalendar, Alert, Checklists, PPEManagement,
   RiskManagement, Schedules, EmergencyManagement, IncidentManagement, Observation,
   CompetencyManagement) — consumer classes exist and are structurally real MassTransit consumers,
   but (a) are never registered with the bus (`AddConsumer<T>()` missing), and (b) even if
   registered, their bodies are idempotency-check-then-log no-ops with zero actual side effects.
3. **Also bad**: The webhook receiver, the one genuine external-ingress point in the whole
   solution, stores the raw payload and does nothing else — no outbox enqueue despite its own
   documentation claiming otherwise.
4. **Read-side symptom of the above**: The Analytics read endpoints (Culture, Dashboard,
   Engagement, PredictiveAnalytics, ScoreCards — 100+ GET endpoints) are almost certainly querying
   projection tables that the corresponding stub consumers never populate, since those 5 consumers
   have an explicit `// TODO: execute … side-effects here`.

**No complete PlantSafety ingress-to-consumed-event flow was found in this pass.** Every
lifecycle-triggering command reaches the database (SQL write via Dapper/stored procedure) —
persistence itself works — but every path beyond persistence (domain event → outbox → bus →
consumer → real side effect) terminates before producing any observable downstream effect.
