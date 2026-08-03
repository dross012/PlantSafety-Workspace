# Gap Analysis — Missing Events (Consumers Without Publishers, and the Full Inverse Table)

Phase 4 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

This document covers two related but distinct things:

1. **Consumers/handlers that exist expecting an event/message type that has no publisher anywhere**
   — the true inverse of `orphaned-ingress.md` (which covers publishers with no consumer). This
   turns out to be a **short list**: this codebase's dominant failure mode is publishers without
   consumers, not the reverse — but the few genuine "phantom consumer" cases found are listed below.
2. **A consolidated table of every "event class defined but never published" and "event published
   but zero consumers" finding** from Phases 1-4, in one place, per the task's request.

---

## Part 1 — Consumers/handlers expecting an event that is never published (the true inverse)

Searching for this pattern required checking, for every registered consumer/handler in the
solution, whether *anything* actually raises the message type it declares. Result:

### 1.1 None found at the intra-module level

Every one of PlantSafety's 22 `*CreatedConsumer` classes, all 18 Azure `INotificationHandler<T>`
classes, and the DataLake/Intelligence outbox processors all correspond to an event type that *is*
raised somewhere in the same module (confirmed via the aggregate/`AddDomainEvent` or
`IPublisher.Publish` call sites cited in `module-flow/*.md`). So there is no case in this codebase
of "we wrote a consumer and then never wrote the corresponding publish call" — every consumer's
counterpart publish site exists. The gap runs entirely the other direction (publish exists,
delivery/consumption doesn't).

### 1.2 The one partial exception — established fact (Phase 3), reframed here as an inverse-consumer gap

`PlantSafety.Infrastructure\BackgroundJobs\OutboxProcessor.cs:21` resolves `IBus` from DI and calls
`bus.Publish(envelope, ...)` where `envelope` is the **raw `OutboxEnvelope` row object**, not the
strongly-typed domain event class (`CultureCreatedEvent`, `AlertCreatedEvent`, etc.) that each
`*CreatedConsumer` actually declares as its `IConsumer<T>` type parameter. This means: even setting
aside the missing `AddConsumer<T>()` registrations (see `orphaned-ingress.md` #3), the message type
MassTransit is asked to route (`OutboxEnvelope`) does not match the message type any of the 22
consumers are typed to receive. **The 22 consumers are, in effect, expecting a message type
(`CultureCreatedEvent`, etc.) that is never the type actually published onto the bus** — the
publish call only ever emits the generic envelope wrapper. This is a distinct, more specific defect
than "consumers are unregistered," and should be tracked separately: registering the consumers
alone would not be sufficient to make this pipeline work, because the type published doesn't match
the type consumed.

### 1.3 Reporting, Azure ML/Networking/etc. — no phantom consumers found

No other module was found to have a consumer/handler referencing an event type with zero raise
sites anywhere in the solution.

---

## Part 2 — Consolidated table: event class defined but never published, or published but zero consumers

All rows below are carried forward from Phases 1-3's module-flow documents (cited) plus the two
**[NEW]** rows added this phase.

### 2a. Event class defined, never instantiated/published anywhere

| Module | Event Class | File (definition) | Basis |
|---|---|---|---|
| DataLake | `RawDataIngestedEvent` | `DataLake.Application.Ingestion.Events` | `module-flow/datalake.md` Flow 1 |
| DataLake | `RawToCleansedCompletedEvent` | `DataLake.Application.Processing.Events` | `module-flow/datalake.md` Flow 2 |
| DataLake | `CleansedToCuratedCompletedEvent` | `DataLake.Application.Processing.Events` | `module-flow/datalake.md` Flow 2 |
| DataLake | (pattern-inferred) very likely every other `*CompletedEvent`/`*Event` class under `DataLake.Application.*.Events` for the 7 read-only controller groups not individually traced | `DataLake.Application.*` | `module-flow/datalake.md` Summary (flagged pattern-inferred, not individually re-verified) |
| Intelligence | (no event raised at all — no class even reaches a publish call) `RunInvestigationCommand` path has no dedicated event class referenced | — | `module-flow/intelligence.md` Flow 1 |
| Intelligence | `DataLakeIngestionCompletedIntegrationEvent` | `Intelligence.Application.DataLake.Events` | `module-flow/intelligence.md` Flow 2 |
| Intelligence | `TrainingJobStartedIntegrationEvent` | `Intelligence.Application.AITraining.Events` | `module-flow/intelligence.md` §AITraining |
| Intelligence | `TrainingJobCancelledIntegrationEvent` | `Intelligence.Application.AITraining.Events` | `module-flow/intelligence.md` §AITraining |
| PlantSafety | `AccidentCreatedEvent` | `PlantSafety.Domain.AccidentManagement.Events` — **this one IS raised/published to the outbox**, listed here only because it has no consumer (see 2b); included for cross-reference | `module-flow/plantsafety.md` §AccidentManagement | 

### 2b. Event published (raised and/or enqueued/sent), zero consumers

| Module | Event Class | Publish Mechanism | Consumer Status | Basis |
|---|---|---|---|---|
| DataLake | `IntegrationEventPublishedEvent` | `IPublisher.Publish` (in-process MediatR notification) | Zero `INotificationHandler<IntegrationEventPublishedEvent>` anywhere | `module-flow/datalake.md` Flow 3 |
| Intelligence | `DataLakeTransformationCompletedIntegrationEvent` | `IIntelligenceEventPublisher.PublishAsync` → Outbox → `OutboxProcessor` → `IOutboxPublisher` | The only `IOutboxPublisher` implementation registered is `NullOutboxPublisher` — logs a warning, marks `Processed=true`, delivers nothing | `module-flow/intelligence.md` Flow 3 |
| PlantSafety | `AccidentCreatedEvent` (+ 6 investigation-lifecycle siblings: `AccidentInvestigatorAssignedEvent`, `AccidentRootCauseRecordedEvent`, `AccidentCorrectiveActionCreatedEvent`, `AccidentCorrectiveActionCompletedEvent`, `AccidentRegulatoryFilingCompletedEvent`, `AccidentInvestigationClosedEvent`) | `AddDomainEvent` → Outbox (via `AccidentDapperRepository.AddAsync`) | **No consumer class of any kind** — not even a stub | `module-flow/plantsafety.md` §AccidentManagement |
| PlantSafety | 22 `*CreatedEvent` types (Culture, Dashboard, Engagement, PredictiveAnalytics, ScoreCards, Auditing, Certification, Compliance, ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar, Alert, Checklists, PPEManagement, RiskManagement, Schedules, EmergencyManagement, IncidentManagement, Observation, CompetencyManagement) | `AddDomainEvent` → Outbox → `bus.Publish(envelope,...)` | Consumer classes exist (real MassTransit `IConsumer<T>`) but **never registered via `AddConsumer<T>()`** — a live RabbitMQ bus with zero bound receive endpoints; and per Part 1.2 above, the type actually published (`OutboxEnvelope`) doesn't even match the consumer's declared type | `module-flow/plantsafety.md` §MassTransit correction; `broker-ingress.md`; `classification-matrix.md` |
| PlantSafety | **[NEW]** All non-Created lifecycle events on the same 22+ aggregates (`Updated`/`Closed`/`Cancelled`/`Completed`/`Assigned`/`Recorded` variants — confirmed present across 26 aggregate files via grep) | Same `AddDomainEvent` → Outbox mechanism | **Zero consumers of any kind** — not even the no-op stub the Created-event siblings got | **[NEW]**, spot-verified this phase (grep across `PlantSafety.Infrastructure` for `*Consumer` classes matching those event-name suffixes returned zero results) |
| Azure | 18 subdomain events (`ContainerAppRestartedEvent`, `CosmosAccountThroughputUpdatedEvent`, etc.) | `IPublisher.Publish` (in-process MediatR) | Each has exactly one same-subdomain `INotificationHandler<T>` that writes a single audit-log line — technically "consumed," but produces no actionable effect and no cross-subdomain/cross-module reaction. Included here because the event's own doc comments (`ContainerAppRestartedEvent`) explicitly note cross-module fan-out was designed for and never built | `module-flow/azure.md` |

### 2c. Outbox/dispatch infrastructure built but never fed (related failure mode, included for completeness)

| Module | Component | Status | Basis |
|---|---|---|---|
| DataLake | `OutboxProcessor`/`OutboxRepository`/`OutboxCleanupService` | Fully wired, hosted, tenant-scoped schema — but zero writes from any Application-layer command (grep confirmed) | `module-flow/datalake.md` §Outbox |
| SafetyIntegrations | `IEventBus`/`IEventBusMessageHandler` (~29 files: dispatcher, backoff, idempotency policies) | Substantial abstraction built, no live caller ever invokes it | `module-flow/safetyintegrations.md` |

---

## Summary counts (this document)

| Category | Count |
|---|---|
| Consumers/handlers expecting an event with no publisher anywhere (Part 1) | 1 confirmed (PlantSafety Outbox type-mismatch, 1.2) — the dominant pattern in this codebase runs the opposite direction |
| Event class defined, never published (2a) | 8 named + 1 "very likely many more, pattern-inferred" note |
| Event published, zero consumers / consumer is a no-op (2b) | 6 rows (30 event types total once the 22-consumer and 18-handler groups are expanded) |
| Outbox/dispatch infra built, never fed (2c) | 2 |

## Single most severe item in this document

The **type mismatch** in PlantSafety's `OutboxProcessor` (§1.2): even if every missing
`AddConsumer<T>()` call were added tomorrow, the 22 `*CreatedConsumer` classes would still never
fire, because the processor publishes the raw `OutboxEnvelope` wrapper, not the strongly-typed
event each consumer is declared against. This means the "just register the consumers" fix that the
rest of this discovery's evidence would suggest is **not sufficient** — the publish call itself
needs to be corrected to publish the deserialized, strongly-typed domain event. Any remediation
plan built only from Phase 1-3 evidence would miss this.
