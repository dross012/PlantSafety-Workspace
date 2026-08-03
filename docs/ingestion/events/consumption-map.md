# Consolidated Consumer Map

Phase 8 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Companion to `publishing-map.md`. Covers every consumer/handler found in Phases 1-7 plus this
phase's direct re-verification of registration status (`AddConsumer<T>()`, `AddHostedService<T>()`,
`IOutboxPublisher` DI registration).

Legend for **Registered?**: **Y** = the consumer/handler is wired into the runtime pipeline that
would actually invoke it (MediatR handler resolution is automatic via assembly scanning for
`INotificationHandler<T>`, so those are "Y" by construction unless explicitly unregistered — none
were found unregistered). **N** = the consumer class exists in source but nothing binds it to a
transport/receive-endpoint, so it is structurally unreachable no matter what is published.

Legend for **Real Side Effect?**: **Y** = performs an observable action (write, notification,
state change, cross-module call). **Stub** = executes but only performs an idempotency check + log
line + mark-processed, no further effect. **N/A (unreachable)** = the code was never wired to run
at all, so "what it does" is moot.

---

## PlantSafety (22 `*CreatedConsumer` classes — all real MassTransit `IConsumer<T>`, all unregistered)

| Consumer | Module | Registered? | Real Side Effect? | Upstream Publisher | file:line |
|---|---|---|---|---|---|
| `CultureCreatedConsumer` | PlantSafety | **N** — `AddConsumer<CultureCreatedConsumer>()` never called in `InfrastructureServiceRegistration.cs` (confirmed: only stub-generator `// TODO: cfg.AddConsumer<...>` lines exist, and only inside agent-worktree scratch copies under `PlantSafety\.claude\worktrees\...`, not the real registration file) | **Stub** (idempotency check → one `ILogger.LogInformation` → mark processed; explicit `// TODO: execute ... side-effects here` per Phase 2) | `CultureCreatedEvent`, raised in `Culture.cs:105` | `PlantSafety\PlantSafety.Infrastructure\Analytics\Culture\Messaging\CultureCreatedConsumer.cs:21` |
| `DashboardCreatedConsumer` | PlantSafety | N | Stub | `DashboardCreatedEvent` | `PlantSafety.Infrastructure\Analytics\Dashboard\Messaging\DashboardCreatedConsumer.cs:21` |
| `EngagementCreatedConsumer` | PlantSafety | N | Stub | `EngagementCreatedEvent` | `PlantSafety.Infrastructure\Analytics\Engagement\Messaging\EngagementCreatedConsumer.cs:21` |
| `PredictiveAnalyticsCreatedConsumer` | PlantSafety | N | Stub | `PredictiveAnalyticsCreatedEvent` | `PlantSafety.Infrastructure\Analytics\PredictiveAnalytics\Messaging\PredictiveAnalyticsCreatedConsumer.cs:21` |
| `ScoreCardsCreatedConsumer` | PlantSafety | N | Stub | `ScoreCardsCreatedEvent` | `PlantSafety.Infrastructure\Analytics\ScoreCards\Messaging\ScoreCardsCreatedConsumer.cs:21` |
| `AuditingCreatedConsumer` | PlantSafety | N | Stub — directly verified (idempotency ledger write to `System.ProcessedEvents` only, no domain write) | `AuditingCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Auditing\Messaging\AuditingCreatedConsumer.cs:21` |
| `CertificationCreatedConsumer` | PlantSafety | N | Stub | `CertificationCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Certification\Messaging\CertificationCreatedConsumer.cs:21` |
| `ComplianceCreatedConsumer` | PlantSafety | N | Stub | `ComplianceCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Compliance\Messaging\ComplianceCreatedConsumer.cs:21` |
| `ManagementOfChangeCreatedConsumer` | PlantSafety | N | Stub | `ManagementOfChangeCreatedEvent` | `PlantSafety.Infrastructure\Compliance\ManagementOfChange\Messaging\ManagementOfChangeCreatedConsumer.cs:21` |
| `PermitsCreatedConsumer` | PlantSafety | N | Stub | `PermitsCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Permits\Messaging\PermitsCreatedConsumer.cs:21` |
| `PolicyCreatedConsumer` | PlantSafety | N | Stub | `PolicyCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Policies\Messaging\PolicyCreatedConsumer.cs:21` |
| `ProceduresCreatedConsumer` | PlantSafety | N | Stub | `ProceduresCreatedEvent` | `PlantSafety.Infrastructure\Compliance\Procedures\Messaging\ProceduresCreatedConsumer.cs:21` |
| `RegulatoryCalendarCreatedConsumer` | PlantSafety | N | Stub | `RegulatoryCalendarCreatedEvent` | `PlantSafety.Infrastructure\Compliance\RegulatoryCalendar\Messaging\RegulatoryCalendarCreatedConsumer.cs:21` |
| `AlertsCreatedConsumer` | PlantSafety | N | Stub — directly verified full read (idempotency check → log → mark processed) | `AlertCreatedEvent` | `PlantSafety.Infrastructure\OperationalSafety\Alerts\Messaging\AlertsCreatedConsumer.cs:21` |
| `ChecklistsCreatedConsumer` | PlantSafety | N | Stub | `ChecklistsCreatedEvent` | `PlantSafety.Infrastructure\OperationalSafety\Checklists\Messaging\ChecklistsCreatedConsumer.cs:21` |
| `PPEManagementCreatedConsumer` | PlantSafety | N | Stub | `PPEManagementCreatedEvent` | `PlantSafety.Infrastructure\OperationalSafety\PPEManagement\Messaging\PPEManagementCreatedConsumer.cs:21` |
| `RiskManagementCreatedConsumer` | PlantSafety | N | Stub | `RiskManagementCreatedEvent` | `PlantSafety.Infrastructure\OperationalSafety\RiskManagement\Messaging\RiskManagementCreatedConsumer.cs:21` |
| `SchedulesCreatedConsumer` | PlantSafety | N | Stub | `SchedulesCreatedEvent` | `PlantSafety.Infrastructure\OperationalSafety\Schedules\Messaging\SchedulesCreatedConsumer.cs:21` |
| `EmergencyManagementCreatedConsumer` | PlantSafety | N | Stub | `EmergencyManagementCreatedEvent` | `PlantSafety.Infrastructure\SafetyEvents\EmergencyManagement\Messaging\EmergencyManagementCreatedConsumer.cs:21` |
| `IncidentManagementCreatedConsumer` | PlantSafety | N | Stub | `IncidentManagementCreatedEvent` | `PlantSafety.Infrastructure\SafetyEvents\IncidentManagement\Messaging\IncidentManagementCreatedConsumer.cs:21` |
| `ObservationCreatedConsumer` | PlantSafety | N | Stub | `ObservationCreatedEvent` | `PlantSafety.Infrastructure\SafetyEvents\Observation\Messaging\ObservationCreatedConsumer.cs:21` |
| `CompetencyManagementCreatedConsumer` | PlantSafety | N | Stub | `CompetencyManagementCreatedEvent` | `PlantSafety.Infrastructure\Training\CompetencyManagement\Messaging\CompetencyManagementCreatedConsumer.cs:20` |
| **(no class)** — `AccidentCreatedEvent` and its 6 investigation-lifecycle siblings have no consumer at all | PlantSafety | **N/A — no consumer class exists to register**, not even a stub | N/A | `AccidentCreatedEvent` + 6 siblings, raised in `Accident.cs` | Grep of `PlantSafety.Infrastructure` for `AccidentCreatedConsumer`/`Accident*Consumer`: zero results |
| **(no class)** — every non-Created lifecycle event on the other ~25 rich aggregates (Alert Acknowledge/Dispatch/Escalate/Resolve/Close, Risk transitions, SafetyPolicy transitions, PermitWorkflow transitions, Scorecard transitions, ChecklistTemplate/Completion transitions, AuditEngagement/AuditFinding transitions, CultureAssessment/StopWorkEvent transitions, PMScheduling/WorkOrderPriority transitions) | PlantSafety | **N/A — no consumer class of any kind exists** | N/A | Same aggregates, non-Create methods | `gaps/orphaned-ingress.md` #4; `lifecycle-coverage/plantsafety-lifecycle.md` |
| `IdempotentConsumerBase<TMessage>` (generic base class both above groups inherit from) | PlantSafety | N/A — it's an abstract base, not itself registered as a consumer | N/A — provides the idempotency-check/log/mark-processed template method that every stub above calls into | N/A | `PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs:21` |

## DataLake

| Consumer | Module | Registered? | Real Side Effect? | Upstream Publisher | file:line |
|---|---|---|---|---|---|
| **(no class)** — `IntegrationEventPublishedEvent` has zero `INotificationHandler<T>` | DataLake | N/A — nothing to register | N/A | `IntegrationEventPublishedEvent`, published by `PublishIntegrationEventCommandHandler` | Grep of `DataLake` for `INotificationHandler<IntegrationEventPublishedEvent>`: zero results |
| **(no class)** — `DataAccessApprovedEvent` has zero `INotificationHandler<T>` | DataLake | N/A | N/A | `DataAccessApprovedEvent`, published by `ApproveDataAccessCommandHandler` (Governance) | Grep of `DataLake` for `INotificationHandler<DataAccessApprovedEvent>`: zero results; `lifecycle-coverage/datalake-lifecycle.md` |
| `OutboxProcessor` (acts as the dispatcher, not a domain consumer) | DataLake | Y — `AddHostedService<OutboxProcessor>()` (`InfrastructureServiceCollectionExtensions.cs:70`) | **N/A (unreachable in practice)** — running, but polls a table nothing ever writes to; there is no message for it to ever dispatch | No publisher — outbox is never fed by any Application command (confirmed by grep) | `DataLake\DataLake.Infrastructure\Outbox\OutboxProcessor.cs:25` |
| `OutboxCleanupService` | DataLake | Y — `AddHostedService<OutboxCleanupService>()` (`InfrastructureServiceCollectionExtensions.cs:71`) | Y (runs its own housekeeping — deletes/archives old processed rows) but operates on a table that's always empty in practice | N/A | `DataLake\DataLake.Infrastructure\Outbox\OutboxCleanupService.cs:22` |

## Intelligence

| Consumer | Module | Registered? | Real Side Effect? | Upstream Publisher | file:line |
|---|---|---|---|---|---|
| `NullOutboxPublisher` (the sole `IOutboxPublisher` implementation — functions as the terminal "consumer" of the outbox pipeline) | Intelligence | Y — `services.AddSingleton<IOutboxPublisher, NullOutboxPublisher>()` (`Intelligence.Infrastructure\DependencyInjection.cs:136`) | **Stub, explicitly** — logs a warning ("NullPublisherRegistered — no event bus implementation has been wired up") and returns `Task.CompletedTask`; `OutboxProcessor` then marks the source row `Processed = true` | `DataLakeTransformationCompletedIntegrationEvent`, published by `TransformDataCommandHandler` via `IIntelligenceEventPublisher.PublishAsync` | `Intelligence\Intelligence.Infrastructure\Persistence\Outbox\NullOutboxPublisher.cs:24` |
| `OutboxProcessor` (dispatcher) | Intelligence | Y — `AddHostedService<OutboxProcessor>()` (`DependencyInjection.cs:137`) | Runs correctly, dispatches to `NullOutboxPublisher` above (i.e. it "delivers" but the terminal handler is a no-op) | `IIntelligenceEventPublisher.PublishAsync` writes | `Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxProcessor.cs:34` |
| **(no call site)** — `TrainingJob` aggregate's own guarded transition methods (`Submit`/`RegisterDataset`/`Start`/`Complete`/`Cancel`/`RecordEvaluation`/`Fail`), each of which would raise a correctly-shaped domain event if invoked | Intelligence | N/A — not a consumer, but included here because it is the *source* of 6 domain event types that never get raised | N/A (never invoked — `StartTrainingJobCommandHandler`/`CancelTrainingJobCommandHandler`/`EvaluateTrainingJobCommandHandler` bypass the aggregate entirely and call the Dapper repository directly) | N/A | `Intelligence.Domain.AITraining.Aggregates.TrainingJob.cs`; `lifecycle-coverage/intelligence-lifecycle.md` |
| **(no call site)** — `Investigation` aggregate's guarded methods (`Open`/`AddFinding`/`SubmitHypothesis`/`Cancel`/`Complete`) | Intelligence | N/A | N/A (never invoked — `CancelInvestigationCommandHandler` calls `_repository.CancelAsync` directly) | N/A | `Intelligence.Domain.AIInvestigations.Aggregates.Investigation.cs` |

## Azure (18 `INotificationHandler<T>` — the only consumers in the solution that reach real running code)

| Consumer | Module | Registered? | Real Side Effect? | Upstream Publisher | file:line |
|---|---|---|---|---|---|
| `ContainerAppRestartedEventHandler` | Azure | Y — MediatR auto-registers `INotificationHandler<T>` via assembly scan (confirmed reached: full read, handler executes) | **Stub-equivalent** — one SIEM-safe `ILogger` line, no further action; own doc comment: "if cross-module notification is required in future, this handler can forward the event to Service Bus" (never built) | `ContainerAppRestartedEvent`, published by `RestartContainerAppCommandHandler` after a live ARM restart succeeds | `Azure\Compute.Application\EventHandlers\ContainerAppRestartedEventHandler.cs:49` |
| `ContainerAppScaleUpdatedEventHandler` | Azure | Y | Stub-equivalent (log-only) | `ContainerAppScaleUpdatedEvent` | `Azure\Compute.Application\EventHandlers\ContainerAppScaleUpdatedEventHandler.cs:54` |
| `CosmosAccountThroughputUpdatedEventHandler` | Azure | Y — directly verified full read | Stub-equivalent (log-only) | `CosmosAccountThroughputUpdatedEvent`, published by `UpdateCosmosAccountThroughputCommandHandler` | `Azure\Data.Application\EventHandlers\CosmosAccountThroughputUpdatedEventHandler.cs:50` |
| `SqlDatabaseTierUpdatedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `SqlDatabaseTierUpdatedEvent` | `Azure\Data.Application\EventHandlers\SqlDatabaseTierUpdatedEventHandler.cs:49` |
| `TopicDeadLetterPurgedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `TopicDeadLetterPurgedEvent` | `Azure\Eventing.Application\EventHandlers\TopicDeadLetterPurgedEventHandler.cs:49` |
| `ManagedIdentityQueriedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `ManagedIdentityQueriedEvent` | `Azure\Identity.Application\EventHandlers\ManagedIdentityQueriedEventHandler.cs:50` |
| `DriftDetectedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `DriftDetectedEvent` | `Azure\ML.Application\EventHandlers\DriftDetectedEventHandler.cs:50` |
| `ModelDeployedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `ModelDeployedEvent` | `Azure\ML.Application\EventHandlers\ModelDeployedEventHandler.cs:46` |
| `TrainingJobCancelledEventHandler` (Azure ML) | Azure | Y (pattern-inferred) | Stub-equivalent | `TrainingJobCancelledEvent` (Azure ML) | `Azure\ML.Application\EventHandlers\TrainingJobCancelledEventHandler.cs:48` |
| `TrainingJobCompletedEventHandler` (Azure ML) | Azure | Y (pattern-inferred) | Stub-equivalent | `TrainingJobCompletedEvent` (Azure ML) | `Azure\ML.Application\EventHandlers\TrainingJobCompletedEventHandler.cs:50` |
| `TrainingJobFailedEventHandler` (Azure ML) | Azure | Y (pattern-inferred) | Stub-equivalent | `TrainingJobFailedEvent` (Azure ML) | `Azure\ML.Application\EventHandlers\TrainingJobFailedEventHandler.cs:50` |
| `TrainingJobSubmittedEventHandler` (Azure ML) | Azure | Y (pattern-inferred) | Stub-equivalent | `TrainingJobSubmittedEvent` (Azure ML) | `Azure\ML.Application\EventHandlers\TrainingJobSubmittedEventHandler.cs:46` |
| `NetworkSecurityGroupUpdatedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `NetworkSecurityGroupUpdatedEvent` | `Azure\Networking.Application\EventHandlers\NetworkSecurityGroupUpdatedEventHandler.cs:22` |
| `PrivateEndpointConnectedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `PrivateEndpointConnectedEvent` | `Azure\Networking.Application\EventHandlers\PrivateEndpointConnectedEventHandler.cs:22` |
| `VirtualNetworkProvisionedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `VirtualNetworkProvisionedEvent` | `Azure\Networking.Application\EventHandlers\VirtualNetworkProvisionedEventHandler.cs:21` |
| `ObservabilityDataQueriedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `ObservabilityDataQueriedEvent` | `Azure\Observability.Application\EventHandlers\ObservabilityDataQueriedEventHandler.cs:42` |
| `SecurityDataAccessedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `SecurityDataAccessedEvent` | `Azure\Security.Application\EventHandlers\SecurityDataAccessedEventHandler.cs:51` |
| `StorageDataAccessedEventHandler` | Azure | Y (pattern-inferred) | Stub-equivalent | `StorageDataAccessedEvent` | `Azure\Storage.Application\EventHandlers\StorageDataAccessedEventHandler.cs:52` |

Note on why Azure's handlers are marked "Stub-equivalent" rather than "Y" under Real Side Effect:
they do execute and do write a real log line (a genuine, if trivial, side effect) — but they
perform no domain-meaningful action (no state change, no notification, no cross-module signal),
which is the bar the task's "does it actually do anything" question is aimed at. They are the only
row in either map where the consumer code provably ran, which is why the summary in
`publishing-map.md` calls this out as a distinct, better-than-everything-else-but-still-trivial
category rather than lumping it in with the fully-unreached stubs.

## SafetyIntegrations

| Consumer/Dispatcher | Module | Registered? | Real Side Effect? | Upstream Publisher | file:line |
|---|---|---|---|---|---|
| `IEventBus`/`IEventBusMessageHandler` implementations (dispatcher, backoff policy, idempotency policy, pipeline middleware — ~29 files) | SafetyIntegrations | **N — built but never hosted/invoked; no live caller found** | N/A (never runs) | No publisher exists — nothing in the solution calls `IEventBus.Publish`/equivalent | `SafetyIntegrations\SafetyIntegrations\EventBus\...` |
| `EnterpriseGatewayInboundAdapter` (implements `IInboundAdapter`) | SafetyIntegrations | **N — no live caller found anywhere** (no HTTP listener, no queue subscription, no scheduled poll invokes `ReceiveAsync`) | N/A (never runs) | No publisher — this is meant to be an inbound boundary from an external system, but nothing external is wired to call it | `SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\EnterpriseGatewayInboundAdapter.cs` |

## ExportImport

No consumer/handler classes of the broker/MediatR-notification shape exist in ExportImport — it
publishes `ImportStarted`/`ImportCompleted`/`OperationFailed` for callers to potentially subscribe
to, but no subscriber was found and, more fundamentally, the publishing code itself is never
reached (module has zero callers solution-wide). Not tabulated as consumer rows for that reason.

---

## Summary counts

| Category | Count |
|---|---|
| Consumers registered and reaching real, business-meaningful code | **0** |
| Consumers registered and reaching code that runs but is a stub/log-only (Azure) | 18 |
| Consumer classes that exist in source but are never registered/reachable (PlantSafety 22 stubs) | 22 |
| Events with zero consumer class of any kind (PlantSafety Accident group ×7 + ~30 non-Created lifecycle events) | ~37 |
| Events published with zero MediatR subscribers (DataLake ×2) | 2 |
| Dispatchers/publishers that are themselves no-op stubs standing in for unbuilt production wiring (Intelligence `NullOutboxPublisher`) | 1 |
| Fully-built dispatch frameworks with no live caller anywhere (SafetyIntegrations `IEventBus`, `EnterpriseGatewayInboundAdapter`) | 2 |
