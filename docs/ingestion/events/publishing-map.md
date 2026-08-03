# Consolidated Outbox / Event-Publishing Map

Phase 8 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Consolidates Phases 1-7 (`module-flow/*.md`, `gaps/missing-events.md`, `gaps/orphaned-ingress.md`,
`lifecycle-coverage/*.md`, `security/rls-audit.md`, `security/actor-propagation.md`,
`broker-ingress.md`) with direct code re-verification of every OutboxProcessor's hosted-service
registration status and the DI wiring around `IOutboxPublisher`/`AddConsumer<T>()`. No fact already
established in Phases 1-7 was re-derived from scratch; this document's job is to line every event
type up in one place and answer the same six questions for each.

---

## Summary

| Metric | Count | Basis |
|---|---|---|
| Event types defined (classes ending `Event`, found via `IDomainEvent`/`INotification`/MassTransit-message usage, across all 9 modules) | **~90+** (22 PlantSafety Created + ~30 PlantSafety non-Created lifecycle siblings, named + pattern-inferred + 7 AccidentManagement + 18 Azure + ~8 DataLake + ~10 Intelligence [incl. 2 dead-domain-model aggregates' 12 event types] + 3 ExportImport) | Consolidated from `broker-ingress.md`, `gaps/missing-events.md`, `lifecycle-coverage/*.md` |
| Event types actually published (instantiated + enqueued/sent by *some* mechanism) | **~59** (22 PlantSafety Created + ~30 PlantSafety non-Created lifecycle + 7 AccidentManagement + 18 Azure — some double counted across categories; see per-module notes) minus the confirmed **never-raised** set (DataLake's 3 named + "likely many more" class, Intelligence's 4 named IntegrationEvents + all 12 TrainingJob/Investigation domain events, ExportImport's 3) | See per-module rows below |
| Event types actually **delivered end-to-end to a consumer that does real work** | **0** | See "Zero end-to-end deliveries" below — this confirms and sharpens the < 1 estimate carried in from Phases 1-7 |
| Event types delivered to *some* consumer, but the consumer only performs a no-op/log-only/idempotency-stub action | **18** (all Azure `INotificationHandler<T>` — the only category where a message actually reaches running consumer code) | `module-flow/azure.md` |

**The true end-to-end delivery count for the whole solution is zero.** No event defined anywhere
in this codebase travels from "raised" through a dispatcher to a consumer that performs a real,
observable side effect (a write, a notification, a state change, a cross-module reaction). The
only category that reaches *any* consumer code at all is Azure's 18 same-subdomain
`INotificationHandler<T>` audit-log writers — and even the event's own doc comments concede this
was meant to be a stepping stone to real cross-module fan-out that was never built. Every other
category dead-ends earlier: unregistered MassTransit consumers (PlantSafety), a `NullOutboxPublisher`
that lies about delivery (Intelligence), an outbox nobody writes to (DataLake), an event published
into MediatR with zero subscribers (DataLake ×2), or an event class that's never even instantiated
(DataLake ×3+, Intelligence ×4, Intelligence's own domain-aggregate events ×12, ExportImport ×3).

## OutboxProcessor operational status per module (re-verified this phase, direct code read)

| Module | Hosted as `IHostedService`/`BackgroundService`? | Evidence | What it actually does when it runs |
|---|---|---|---|
| PlantSafety | **Yes — running** | `PlantSafety.Infrastructure\DependencyInjection\InfrastructureServiceRegistration.cs:240` — `services.AddHostedService<OutboxProcessor>();` | Polls the Outbox table and calls `bus.Publish(envelope, ...)` where `envelope` is the raw `OutboxEnvelope` row — not the strongly-typed event any of the 22 consumers declare (`gaps/missing-events.md` §1.2). Even if `AddConsumer<T>()` calls were added, the published type would still not match what the consumers expect. |
| DataLake | **Yes — running** (both processor and cleanup service) | `DataLake\DataLake.Infrastructure\DependencyInjection\InfrastructureServiceCollectionExtensions.cs:70-71` — `services.AddHostedService<OutboxProcessor>(); services.AddHostedService<OutboxCleanupService>();` | Polls a table that is never written to by any Application-layer command (`module-flow/datalake.md` §Outbox) — runs continuously, finds nothing, does nothing. Confirmed dormant, not broken. |
| Intelligence | **Yes — running** | `Intelligence\Intelligence.Infrastructure\DependencyInjection.cs:137` — `services.AddHostedService<OutboxProcessor>();` | Polls its Outbox (which **is** written to, correctly, by `TransformDataCommandHandler`), and for every row calls `IOutboxPublisher.PublishAsync`. The only registered `IOutboxPublisher` is `NullOutboxPublisher` (`Intelligence.Infrastructure.DependencyInjection.cs:136` — `services.AddSingleton<IOutboxPublisher, NullOutboxPublisher>();`), which logs a warning and returns `Task.CompletedTask`. The processor then marks the row `Processed = true`. **This is the one case in the solution where the dispatcher runs, finds real work, and actively mis-reports it as delivered.** |
| Azure | **No outbox exists** — Azure uses in-process MediatR `IPublisher.Publish` directly from the command handler to the `INotificationHandler<T>`, synchronously, no durable queue/dispatcher in between | `module-flow/azure.md` | N/A — there is no outbox to poll; delivery (to the same-subdomain audit-log handler) is immediate and reliable, it just terminates trivially |
| SafetyIntegrations | **No** — `IEventBus`/`IEventBusMessageHandler` abstraction exists (~29 files) but nothing hosts/dispatches it and no live caller invokes it | `module-flow/safetyintegrations.md`, `gaps/missing-events.md` §2c | N/A — never runs |
| ExportImport | **No outbox/dispatcher of any kind** — the module publishes `ImportStarted`/`ImportCompleted`/`OperationFailed` events only if a command handler is ever invoked, and none is (module has zero callers, no `*.Api` project) | `gaps/orphaned-ingress.md` §ExportImport | N/A — module is entirely unreached |

---

## Event Type Table

Legend for **Published?**: **Y** = an event instance is constructed and handed to a
publish/enqueue mechanism somewhere in the live code path. **N** = the class is defined but no
call site anywhere constructs and publishes/enqueues it.

Legend for **Outbox-Delivered?**: **Y** = a dispatcher (OutboxProcessor, MediatR pipeline, or
direct in-process call) actually hands the message to a **real, functioning** consumer that
performs observable work beyond logging/marking-processed. **N (reason)** = delivery mechanism
exists but fails to reach a working consumer, with the specific reason given. **N/A** = never
published, so there is nothing to deliver.

### PlantSafety

| Event Type | Module | Published? | Outbox-Delivered? | file:line |
|---|---|---|---|---|
| `CultureCreatedEvent` | PlantSafety | Y — `AddDomainEvent` in `Culture.cs:105` → `OutboxWriter.Enqueue` | N (consumer `CultureCreatedConsumer` exists but `AddConsumer<CultureCreatedConsumer>()` is never called; even if it were, `OutboxProcessor` publishes the raw `OutboxEnvelope` type, not `CultureCreatedEvent`, so the consumer's declared type would never match anyway) | `PlantSafety\PlantSafety.Domain\Analytics\Culture\Aggregates\Culture.cs:105`; consumer at `PlantSafety\PlantSafety.Infrastructure\Analytics\Culture\Messaging\CultureCreatedConsumer.cs:21`; processor at `PlantSafety\PlantSafety.Infrastructure\BackgroundJobs\OutboxProcessor.cs:21` |
| `DashboardCreatedEvent` | PlantSafety | Y (same mechanism) | N (same double-defect: unregistered + type-mismatch) | Consumer: `PlantSafety.Infrastructure\Analytics\Dashboard\Messaging\DashboardCreatedConsumer.cs:21` |
| `EngagementCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Analytics\Engagement\Messaging\EngagementCreatedConsumer.cs:21` |
| `PredictiveAnalyticsCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Analytics\PredictiveAnalytics\Messaging\PredictiveAnalyticsCreatedConsumer.cs:21` |
| `ScoreCardsCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Analytics\ScoreCards\Messaging\ScoreCardsCreatedConsumer.cs:21` |
| `AuditingCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Auditing\Messaging\AuditingCreatedConsumer.cs:21` |
| `CertificationCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Certification\Messaging\CertificationCreatedConsumer.cs:21` |
| `ComplianceCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Compliance\Messaging\ComplianceCreatedConsumer.cs:21` |
| `ManagementOfChangeCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\ManagementOfChange\Messaging\ManagementOfChangeCreatedConsumer.cs:21` |
| `PermitsCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Permits\Messaging\PermitsCreatedConsumer.cs:21` |
| `PolicyCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Policies\Messaging\PolicyCreatedConsumer.cs:21` |
| `ProceduresCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\Procedures\Messaging\ProceduresCreatedConsumer.cs:21` |
| `RegulatoryCalendarCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Compliance\RegulatoryCalendar\Messaging\RegulatoryCalendarCreatedConsumer.cs:21` |
| `AlertCreatedEvent` | PlantSafety | Y — `Alert.Trigger(...)` (pattern-inferred enqueue call site) | N (consumer `AlertsCreatedConsumer` unregistered + type-mismatch; body is idempotency-check-then-log even if reached) | `PlantSafety.Infrastructure\OperationalSafety\Alerts\Messaging\AlertsCreatedConsumer.cs:21` |
| `ChecklistsCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\OperationalSafety\Checklists\Messaging\ChecklistsCreatedConsumer.cs:21` |
| `PPEManagementCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\OperationalSafety\PPEManagement\Messaging\PPEManagementCreatedConsumer.cs:21` |
| `RiskManagementCreatedEvent` (Operational marker aggregate, distinct from the live `Risk`) | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\OperationalSafety\RiskManagement\Messaging\RiskManagementCreatedConsumer.cs:21` |
| `SchedulesCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\OperationalSafety\Schedules\Messaging\SchedulesCreatedConsumer.cs:21` |
| `EmergencyManagementCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\SafetyEvents\EmergencyManagement\Messaging\EmergencyManagementCreatedConsumer.cs:21` |
| `IncidentManagementCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\SafetyEvents\IncidentManagement\Messaging\IncidentManagementCreatedConsumer.cs:21` |
| `ObservationCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\SafetyEvents\Observation\Messaging\ObservationCreatedConsumer.cs:21` |
| `CompetencyManagementCreatedEvent` | PlantSafety | Y | N (same) | `PlantSafety.Infrastructure\Training\CompetencyManagement\Messaging\CompetencyManagementCreatedConsumer.cs:20` |
| `AccidentCreatedEvent` | PlantSafety | Y — `AddDomainEvent` in `Accident.Create` → `AccidentDapperRepository.AddAsync` → `outboxWriter.Enqueue` | **N (no consumer class exists at all — not even a stub)** | `PlantSafety.Domain\AccidentManagement\Aggregates\Accident.cs` (raise site); `PlantSafety.Infrastructure\AccidentManagement\...\AccidentDapperRepository.cs:124` (enqueue) |
| `AccidentInvestigatorAssignedEvent` | PlantSafety | Y (same mechanism) | N (no consumer at all) | `Accident.cs` |
| `AccidentRootCauseRecordedEvent` | PlantSafety | Y | N (no consumer at all) | `Accident.cs` |
| `AccidentCorrectiveActionCreatedEvent` | PlantSafety | Y | N (no consumer at all) | `Accident.cs` |
| `AccidentCorrectiveActionCompletedEvent` | PlantSafety | Y | N (no consumer at all) | `Accident.cs` |
| `AccidentRegulatoryFilingCompletedEvent` | PlantSafety | Y | N (no consumer at all) | `Accident.cs` |
| `AccidentInvestigationClosedEvent` | PlantSafety | Y | N (no consumer at all) | `Accident.cs` |
| All non-Created lifecycle events on the other ~25 rich/asymmetric aggregates (`AlertAcknowledgedEvent`, `AlertClosedEvent`, `RiskCreatedEvent`/`ScoreRisk`/`Prioritize`/`ApplyControlMeasure`/`ValidateEffectiveness`/`Close` events, `SafetyPolicy` Submit/Approve/Reject/Publish/Retire events, `PermitWorkflow` AssessHazards/Approve/Issue/Reject/Expire events, `Scorecard` RecordMetric/CompleteCompilation/Publish/Close events, `ChecklistTemplate`/`ChecklistCompletion` Publish/Start/Complete events, `AuditEngagement`/`AuditFinding` Schedule/Start/RecordFinding/Close/Remediate/Verify/TriggerCapa events, `CultureAssessment`/`StopWorkEvent` events, `PMSchedulingCancelledEvent` + `WorkOrderPriority` events, PPE/Observability lifecycle events — ~30+ distinct event types by name) | PlantSafety | Y — same `AddDomainEvent` → `outboxWriter.Enqueue` mechanism, confirmed present via grep across 26 aggregate files | **N — zero consumers of any kind**, not even the no-op stub the 22 Created siblings got (grep for `*Consumer` matching `Updated\|Deleted\|Cancelled\|Closed\|Acknowledged\|...` returned zero results) | Representative: `PlantSafety.Domain\OperationalSafety\Alerts\Alert\Aggregates\Alert.cs`, `PlantSafety.Domain\RiskManagement\Aggregates\Risk.cs`, `PlantSafety.Domain\Compliance\Auditing\...` (see `lifecycle-coverage/plantsafety-lifecycle.md` for the full per-aggregate breakdown) |
| `Accident.Archive` / `Risk.Archive` / `Scorecard.Archive` (Archive transitions — guarded, but never call `AddDomainEvent`) | PlantSafety | **N — the transition itself never constructs an event** (confirmed directly for 3 aggregates; SafetyPolicy/PermitWorkflow archive event-status unverified but suspected same) | N/A | `lifecycle-coverage/plantsafety-lifecycle.md` — "Archive-never-publishes" cross-cutting finding |

### DataLake

| Event Type | Module | Published? | Outbox-Delivered? | file:line |
|---|---|---|---|---|
| `RawDataIngestedEvent` | DataLake | **N — never instantiated anywhere** | N/A | `DataLake.Application.Ingestion.Events` (definition only); handler at `DataLake\DataLake.Api\Controllers\v1\IngestionController.cs:20` never references it |
| `RawToCleansedCompletedEvent` | DataLake | **N** | N/A | `DataLake.Application.Processing.Events` (definition only) |
| `CleansedToCuratedCompletedEvent` | DataLake | **N** (pattern-inferred, same grep as above) | N/A | `DataLake.Application.Processing.Events` (definition only) |
| Every other `*CompletedEvent`/`*Event` class under `DataLake.Application.*.Events` for the 7 read-only controller groups (Archival, Consumption, DataQuality, Governance, MetadataCatalog, Schema) | DataLake | **N (pattern-inferred, not individually re-verified per class)** | N/A | `module-flow/datalake.md` Summary |
| `IntegrationEventPublishedEvent` | DataLake | Y — `IPublisher.Publish` in `PublishIntegrationEventCommandHandler` (in-process MediatR) | **N — zero `INotificationHandler<IntegrationEventPublishedEvent>` anywhere** | `DataLake\DataLake.Api\Controllers\v1\IntegrationController.cs:20` |
| `DataAccessApprovedEvent` | DataLake | Y — `IPublisher.Publish` in `ApproveDataAccessCommandHandler` (Governance) | **N — zero `INotificationHandler<DataAccessApprovedEvent>` anywhere** (confirmed Phase 6) | `lifecycle-coverage/datalake-lifecycle.md` §Access Request (Governance) |
| DataLake Outbox (`OutboxMessage`/`OutboxProcessor`/`OutboxRepository`/`OutboxCleanupService`) | DataLake | N/A — infrastructure, not an event type | N — hosted and polling (confirmed `AddHostedService<OutboxProcessor>()`/`AddHostedService<OutboxCleanupService>()` at `InfrastructureServiceCollectionExtensions.cs:70-71`), but zero writes from any Application-layer command (grep-confirmed) | `DataLake\DataLake.Infrastructure\Outbox\OutboxProcessor.cs:25` |

### Intelligence

| Event Type | Module | Published? | Outbox-Delivered? | file:line |
|---|---|---|---|---|
| `DataLakeIngestionCompletedIntegrationEvent` | Intelligence | **N — never referenced by any handler** | N/A | `Intelligence.Application.DataLake.Events` (definition only); handler at `Intelligence\Intelligence.Api\Controllers\v1\DataLakeController.cs:25` |
| `TrainingJobStartedIntegrationEvent` | Intelligence | **N** | N/A | `Intelligence.Application.AITraining.Events` (definition only) |
| `TrainingJobCancelledIntegrationEvent` | Intelligence | **N** | N/A | `Intelligence.Application.AITraining.Events` (definition only) |
| `DataLakeTransformationCompletedIntegrationEvent` | Intelligence | Y — `IIntelligenceEventPublisher.PublishAsync` in `TransformDataCommandHandler` → written to Outbox correctly | **N — `OutboxProcessor` (confirmed hosted, `Intelligence.Infrastructure\DependencyInjection.cs:137`) drains the row and hands it to the only registered `IOutboxPublisher`, `NullOutboxPublisher` (`DependencyInjection.cs:136`), which logs a warning and marks `Processed=true` without delivering** | `Intelligence\Intelligence.Api\Controllers\v1\DataLakeController.cs:46`; publisher at `Intelligence.Infrastructure\Messaging\IntelligenceEventPublisher.cs`; null impl at `Intelligence.Infrastructure\Persistence\Outbox\NullOutboxPublisher.cs:24` |
| `TrainingJobStartedEvent`/`RunningEvent`/`CompletedEvent`/`CancelledEvent`/`EvaluatedEvent`/`FailedEvent` (raised inside the `TrainingJob` domain aggregate's guarded methods) | Intelligence | **N at runtime** — the aggregate's own methods would raise these correctly, but no Application-layer handler ever constructs/loads a `TrainingJob` instance; every handler (`StartTrainingJobCommandHandler`, `CancelTrainingJobCommandHandler`, `EvaluateTrainingJobCommandHandler`) calls the Dapper repository directly against a stored procedure instead | N/A — dead domain code, never reached | `Intelligence.Domain.AITraining.Aggregates.TrainingJob.cs` (full guarded state machine, unreachable); `lifecycle-coverage/intelligence-lifecycle.md` headline finding |
| `Investigation`'s domain events (Open/AddFinding/SubmitHypothesis/Cancel/Complete) | Intelligence | **N at runtime** — same aggregate-bypass pattern; `CancelInvestigationCommandHandler` calls `_repository.CancelAsync` directly, never touching `Investigation.Cancel` | N/A — dead domain code | `Intelligence.Domain.AIInvestigations.Aggregates.Investigation.cs`; `lifecycle-coverage/intelligence-lifecycle.md` |
| Intelligence Outbox infra (`OutboxMessage`/`OutboxProcessor`/`OutboxRepository`/`OutboxPublishingResiliencePipeline`) | Intelligence | N/A — infrastructure | Hosted and running (confirmed), correctly fed by exactly one command (`TransformDataCommand`) — see above row for the terminal `NullOutboxPublisher` defect | `Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxProcessor.cs:34` |

### Azure

| Event Type | Module | Published? | Outbox-Delivered? | file:line |
|---|---|---|---|---|
| `ContainerAppRestartedEvent` | Azure | Y — `IPublisher.Publish` after a live ARM restart call succeeds | Y (delivered), but consumer is log-only (same-subdomain audit line, no cross-module/actionable effect; own doc comment flags cross-module fan-out as "future work" never built) | `Azure\Compute.Application\EventHandlers\ContainerAppRestartedEventHandler.cs:49` |
| `ContainerAppScaleUpdatedEvent` | Azure | Y | Y (log-only) | `Azure\Compute.Application\EventHandlers\ContainerAppScaleUpdatedEventHandler.cs:54` |
| `CosmosAccountThroughputUpdatedEvent` | Azure | Y | Y (log-only) | `Azure\Data.Application\EventHandlers\CosmosAccountThroughputUpdatedEventHandler.cs:50` |
| `SqlDatabaseTierUpdatedEvent` | Azure | Y (pattern-inferred) | Y (log-only, pattern-inferred) | `Azure\Data.Application\EventHandlers\SqlDatabaseTierUpdatedEventHandler.cs:49` |
| `TopicDeadLetterPurgedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Eventing.Application\EventHandlers\TopicDeadLetterPurgedEventHandler.cs:49` |
| `ManagedIdentityQueriedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Identity.Application\EventHandlers\ManagedIdentityQueriedEventHandler.cs:50` |
| `DriftDetectedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\DriftDetectedEventHandler.cs:50` |
| `ModelDeployedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\ModelDeployedEventHandler.cs:46` |
| `TrainingJobCancelledEvent` (Azure ML, distinct from Intelligence's) | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\TrainingJobCancelledEventHandler.cs:48` |
| `TrainingJobCompletedEvent` (Azure ML) | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\TrainingJobCompletedEventHandler.cs:50` |
| `TrainingJobFailedEvent` (Azure ML) | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\TrainingJobFailedEventHandler.cs:50` |
| `TrainingJobSubmittedEvent` (Azure ML) | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\ML.Application\EventHandlers\TrainingJobSubmittedEventHandler.cs:46` |
| `NetworkSecurityGroupUpdatedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Networking.Application\EventHandlers\NetworkSecurityGroupUpdatedEventHandler.cs:22` |
| `PrivateEndpointConnectedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Networking.Application\EventHandlers\PrivateEndpointConnectedEventHandler.cs:22` |
| `VirtualNetworkProvisionedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Networking.Application\EventHandlers\VirtualNetworkProvisionedEventHandler.cs:21` |
| `ObservabilityDataQueriedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Observability.Application\EventHandlers\ObservabilityDataQueriedEventHandler.cs:42` |
| `SecurityDataAccessedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Security.Application\EventHandlers\SecurityDataAccessedEventHandler.cs:51` |
| `StorageDataAccessedEvent` | Azure | Y (pattern-inferred) | Y (log-only) | `Azure\Storage.Application\EventHandlers\StorageDataAccessedEventHandler.cs:52` |

### ExportImport (module has zero live callers — included for completeness)

| Event Type | Module | Published? | Outbox-Delivered? | file:line |
|---|---|---|---|---|
| `ImportStarted` | ExportImport | **N — would publish if the module were ever invoked; nothing calls `StartImportCommandHandler`/`ImportPipelineService` anywhere in the solution** | N/A | `ExportImport\ExportImport.Application\Services\Import\ImportPipelineService.cs:31`; `gaps/orphaned-ingress.md` §ExportImport |
| `ImportCompleted` | ExportImport | **N (same reachability gap)** | N/A | same |
| `OperationFailed` | ExportImport | **N (same reachability gap)** | N/A | same |

### SafetyIntegrations

No event classes of the `*Event`/`IDomainEvent` shape were found in SafetyIntegrations; its
`IEventBus`/`IEventBusMessageHandler` abstraction (~29 files) is a dispatch framework, not a set of
event contracts, and has no live caller (`gaps/missing-events.md` §2c). Not tabulated as event
rows for that reason — see `consumption-map.md` for its dispatcher/handler inventory.

---

## Zero end-to-end deliveries — why the number is exactly 0, not "very low"

Every category was checked for a path that both (a) reaches running consumer code and (b) that
code does something beyond logging/marking a row processed:

1. **PlantSafety's 22 Created + ~7 Accident + ~30 non-Created lifecycle events**: either no
   consumer class exists, or the consumer exists but is never bound to a receive endpoint
   (`AddConsumer<T>()` missing) — and even a hypothetical fix wouldn't be sufficient, because
   `OutboxProcessor` publishes the wrong CLR type (`OutboxEnvelope`, not the domain event).
2. **DataLake's defined-but-unpublished events (RawData/RawToCleansed/CleansedToCurated + the
   pattern-inferred rest)**: never even instantiated.
3. **DataLake's two genuinely-published events (`IntegrationEventPublishedEvent`,
   `DataAccessApprovedEvent`)**: published correctly into MediatR, zero subscribers registered.
4. **Intelligence's defined-but-unpublished events + the 12 dead-aggregate domain events**: never
   instantiated (handlers bypass the aggregate entirely).
5. **Intelligence's one genuinely end-to-end-attempted event
   (`DataLakeTransformationCompletedIntegrationEvent`)**: reaches a running `OutboxProcessor`,
   which hands it to `NullOutboxPublisher` — the closest thing to "delivered" in the whole
   solution, and it is explicitly a no-op that lies about success.
6. **Azure's 18 events**: the only events in the entire solution that reach real, running consumer
   code — but that code is a single `ILogger` line, by explicit design (per each handler's own
   header comment), with cross-module fan-out documented as planned-but-never-built.
7. **ExportImport's 3 events**: the publishing code exists and would work, but the module has no
   caller anywhere in the solution, so the code never executes.

No event in this codebase reaches a consumer that performs a business-meaningful action (a write,
a cross-module signal, a notification, a projection update, a workflow trigger). The Azure case is
the closest thing to a "working" pipeline and even it was designed knowing its terminus was
trivial.

## Event loops

**None found.** A loop requires at least one event to reach a consumer that itself publishes a
further event as a reaction. Since no event in this solution reaches a consumer that does
anything beyond logging or marking a row processed (see above), there is no live code path by
which any event's consumption could trigger a second publish — the precondition for a loop does
not exist anywhere in the current codebase. This should be re-checked if any of the dead-ends
above (unregistered consumers, `NullOutboxPublisher`, unfed DataLake outbox) are ever fixed in
isolation without a broader review, since fixing PlantSafety's 22 consumers to "do real work" is
exactly the kind of change that could introduce a new republish-on-consume path; no such path
exists today.
