# Broker / Event-Consumer Ingress — Ingestion Pipeline Discovery (Phase 1)

Extracted by grepping all `.cs` files for `IConsumer<T>`, `ConsumeContext<T>`, `.Subscribe(`,
`INotificationHandler<T>`/`IDomainEventHandler<T>`, and classes whose name contains `Outbox`.
File paths are relative to `C:\Users\dross\source`.

**Framework found:** No MassTransit/Azure Service Bus/RabbitMQ consumer registrations were found
anywhere in the scanned tree — despite `IConsumer<T>` / `ConsumeContext<T>` usage in PlantSafety,
these are a **custom in-process messaging abstraction** (see
`PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs`), not MassTransit's
actual bus. No literal `MassTransit` namespace usage was found in any of the 14,090 `.cs` files
scanned (searched, not found — treat any MassTransit claim as unverified).

**Publisher determination:** For the PlantSafety `*CreatedEvent` / `*CreatedConsumer` pairs, the
event is raised as a **domain event** on the owning aggregate (e.g.
`PlantSafety.Domain\Analytics\Culture\Aggregates\Culture.cs:105` calls
`agg.AddDomainEvent(new CultureCreatedEvent(...))`), then written to the Outbox
(`PlantSafety.Infrastructure\Persistence\UnitOfWork\OutboxWriter.cs`) and dispatched by
`PlantSafety.Infrastructure\BackgroundJobs\OutboxProcessor.cs`. The matching `*CreatedConsumer`
lives in the **same module** (PlantSafety.Infrastructure), so this reads as an intra-module
CQRS/outbox projection dispatch pattern rather than genuine cross-module bus messaging — no
evidence was found of another module (e.g. DataLake, Intelligence) consuming any PlantSafety event
or vice versa. This should be treated as **unverified/needs follow-up** for Phase 6
(cross-module integration validation), not confirmed cross-module wiring.

**SafetyIntegrations note:** SafetyIntegrations has its own custom `IEventBus` /
`IEventBusMessageHandler` abstraction (`SafetyIntegrations\SafetyIntegrations\EventBus\...`) plus a
single governed external-entry interface, `IInboundAdapter`
(`SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\IInboundAdapter.cs`), implemented by
`EnterpriseGatewayInboundAdapter.cs`. This is the closest thing in the codebase to a documented
"single front door" for inbound integration traffic. No concrete registration wiring this adapter
to a live transport (HTTP listener, queue, etc.) was found — it's an adapter class waiting to be
invoked by a caller not yet located. Flagged as **unverified/needs follow-up**.

Three additional matches (`OutboxProcessorTests`, `OutboxPersistenceTests`,
`OutboxFailoverAndRecoveryTests`, all in `PlantSafety.IntegrationTests`/`PlantSafety.ResilienceTests`)
are test coverage of the outbox pattern, not production ingress points, and are excluded from the
table below (see inventory.md for the raw count including tests).

Row count (production code only): 79.

| Module | Kind | Message/Contract Type | File:Line |
|---|---|---|---|
| Azure | INotificationHandler<T> | `ContainerAppRestartedEvent` | Azure\Compute.Application\EventHandlers\ContainerAppRestartedEventHandler.cs:49 |
| Azure | INotificationHandler<T> | `ContainerAppScaleUpdatedEvent` | Azure\Compute.Application\EventHandlers\ContainerAppScaleUpdatedEventHandler.cs:54 |
| Azure | INotificationHandler<T> | `CosmosAccountThroughputUpdatedEvent` | Azure\Data.Application\EventHandlers\CosmosAccountThroughputUpdatedEventHandler.cs:50 |
| Azure | INotificationHandler<T> | `SqlDatabaseTierUpdatedEvent` | Azure\Data.Application\EventHandlers\SqlDatabaseTierUpdatedEventHandler.cs:49 |
| Azure | INotificationHandler<T> | `TopicDeadLetterPurgedEvent` | Azure\Eventing.Application\EventHandlers\TopicDeadLetterPurgedEventHandler.cs:49 |
| Azure | INotificationHandler<T> | `ManagedIdentityQueriedEvent` | Azure\Identity.Application\EventHandlers\ManagedIdentityQueriedEventHandler.cs:50 |
| Azure | INotificationHandler<T> | `DriftDetectedEvent` | Azure\ML.Application\EventHandlers\DriftDetectedEventHandler.cs:50 |
| Azure | INotificationHandler<T> | `ModelDeployedEvent` | Azure\ML.Application\EventHandlers\ModelDeployedEventHandler.cs:46 |
| Azure | INotificationHandler<T> | `TrainingJobCancelledEvent` | Azure\ML.Application\EventHandlers\TrainingJobCancelledEventHandler.cs:48 |
| Azure | INotificationHandler<T> | `TrainingJobCompletedEvent` | Azure\ML.Application\EventHandlers\TrainingJobCompletedEventHandler.cs:50 |
| Azure | INotificationHandler<T> | `TrainingJobFailedEvent` | Azure\ML.Application\EventHandlers\TrainingJobFailedEventHandler.cs:50 |
| Azure | INotificationHandler<T> | `TrainingJobSubmittedEvent` | Azure\ML.Application\EventHandlers\TrainingJobSubmittedEventHandler.cs:46 |
| Azure | INotificationHandler<T> | `NetworkSecurityGroupUpdatedEvent` | Azure\Networking.Application\EventHandlers\NetworkSecurityGroupUpdatedEventHandler.cs:22 |
| Azure | INotificationHandler<T> | `PrivateEndpointConnectedEvent` | Azure\Networking.Application\EventHandlers\PrivateEndpointConnectedEventHandler.cs:22 |
| Azure | INotificationHandler<T> | `VirtualNetworkProvisionedEvent` | Azure\Networking.Application\EventHandlers\VirtualNetworkProvisionedEventHandler.cs:21 |
| Azure | INotificationHandler<T> | `ObservabilityDataQueriedEvent` | Azure\Observability.Application\EventHandlers\ObservabilityDataQueriedEventHandler.cs:42 |
| Azure | INotificationHandler<T> | `SecurityDataAccessedEvent` | Azure\Security.Application\EventHandlers\SecurityDataAccessedEventHandler.cs:51 |
| Azure | INotificationHandler<T> | `StorageDataAccessedEvent` | Azure\Storage.Application\EventHandlers\StorageDataAccessedEventHandler.cs:52 |
| DataLake | Outbox class | `OutboxCleanupService` | DataLake\DataLake.Infrastructure\Outbox\OutboxCleanupService.cs:22 |
| DataLake | Outbox class | `OutboxMessage` | DataLake\DataLake.Infrastructure\Outbox\OutboxMessage.cs:17 |
| DataLake | Outbox class | `OutboxProcessor` | DataLake\DataLake.Infrastructure\Outbox\OutboxProcessor.cs:25 |
| DataLake | Outbox class | `OutboxRepository` | DataLake\DataLake.Infrastructure\Outbox\OutboxRepository.cs:18 |
| Intelligence | Outbox class | `OutboxMessage` | Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxMessage.cs:19 |
| Intelligence | Outbox class | `OutboxProcessor` | Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxProcessor.cs:34 |
| Intelligence | Outbox class | `OutboxRepository` | Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxRepository.cs:29 |
| Intelligence | Outbox class | `OutboxPublishingResiliencePipeline` | Intelligence\Intelligence.Infrastructure\Shared\Resilience\OutboxPublishingResiliencePipeline.cs:23 |
| PlantSafety | IConsumer<T> | `CultureCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Culture\Messaging\CultureCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `CultureCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Culture\Messaging\CultureCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `DashboardCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Dashboard\Messaging\DashboardCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `DashboardCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Dashboard\Messaging\DashboardCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `EngagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Engagement\Messaging\EngagementCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `EngagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\Engagement\Messaging\EngagementCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `PredictiveAnalyticsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\PredictiveAnalytics\Messaging\PredictiveAnalyticsCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `PredictiveAnalyticsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\PredictiveAnalytics\Messaging\PredictiveAnalyticsCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ScoreCardsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\ScoreCards\Messaging\ScoreCardsCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ScoreCardsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Analytics\ScoreCards\Messaging\ScoreCardsCreatedConsumer.cs:24 |
| PlantSafety | Outbox class | `OutboxProcessor` | PlantSafety\PlantSafety.Infrastructure\BackgroundJobs\OutboxProcessor.cs:21 |
| PlantSafety | Outbox class | `OutboxPipelineBehavior` | PlantSafety\PlantSafety.Infrastructure\Common\Pipelines\OutboxPipelineBehavior.cs:21 |
| PlantSafety | IConsumer<T> | `AuditingCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Auditing\Messaging\AuditingCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `AuditingCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Auditing\Messaging\AuditingCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `CertificationCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Certification\Messaging\CertificationCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `CertificationCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Certification\Messaging\CertificationCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ComplianceCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Compliance\Messaging\ComplianceCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ComplianceCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Compliance\Messaging\ComplianceCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ManagementOfChangeCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\ManagementOfChange\Messaging\ManagementOfChangeCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ManagementOfChangeCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\ManagementOfChange\Messaging\ManagementOfChangeCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `PermitsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Permits\Messaging\PermitsCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `PermitsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Permits\Messaging\PermitsCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `PolicyCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Policies\Messaging\PolicyCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `PolicyCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Policies\Messaging\PolicyCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ProceduresCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Procedures\Messaging\ProceduresCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ProceduresCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\Procedures\Messaging\ProceduresCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `RegulatoryCalendarCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\RegulatoryCalendar\Messaging\RegulatoryCalendarCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `RegulatoryCalendarCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Compliance\RegulatoryCalendar\Messaging\RegulatoryCalendarCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `TMessage` | PlantSafety\PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs:21 |
| PlantSafety | ConsumeContext<T> | `TMessage` | PlantSafety\PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs:25 |
| PlantSafety | ConsumeContext<T> | `TMessage` | PlantSafety\PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs:70 |
| PlantSafety | IConsumer<T> | `AlertCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Alerts\Messaging\AlertsCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `AlertCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Alerts\Messaging\AlertsCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ChecklistsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Checklists\Messaging\ChecklistsCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ChecklistsCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Checklists\Messaging\ChecklistsCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `PPEManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\PPEManagement\Messaging\PPEManagementCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `PPEManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\PPEManagement\Messaging\PPEManagementCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `RiskManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\RiskManagement\Messaging\RiskManagementCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `RiskManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\RiskManagement\Messaging\RiskManagementCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `SchedulesCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Schedules\Messaging\SchedulesCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `SchedulesCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\OperationalSafety\Schedules\Messaging\SchedulesCreatedConsumer.cs:24 |
| PlantSafety | Outbox class | `OutboxWriter` | PlantSafety\PlantSafety.Infrastructure\Persistence\UnitOfWork\OutboxWriter.cs:24 |
| PlantSafety | IConsumer<T> | `EmergencyManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\EmergencyManagement\Messaging\EmergencyManagementCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `EmergencyManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\EmergencyManagement\Messaging\EmergencyManagementCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `IncidentManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\IncidentManagement\Messaging\IncidentManagementCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `IncidentManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\IncidentManagement\Messaging\IncidentManagementCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `ObservationCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\Observation\Messaging\ObservationCreatedConsumer.cs:21 |
| PlantSafety | ConsumeContext<T> | `ObservationCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\SafetyEvents\Observation\Messaging\ObservationCreatedConsumer.cs:24 |
| PlantSafety | IConsumer<T> | `CompetencyManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Training\CompetencyManagement\Messaging\CompetencyManagementCreatedConsumer.cs:20 |
| PlantSafety | ConsumeContext<T> | `CompetencyManagementCreatedEvent` | PlantSafety\PlantSafety.Infrastructure\Training\CompetencyManagement\Messaging\CompetencyManagementCreatedConsumer.cs:23 |
