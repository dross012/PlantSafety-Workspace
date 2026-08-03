# Gap Analysis — Orphaned Ingress Points

Phase 4 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `inventory.md`, `api-ingress.md`, `broker-ingress.md`, `file-ingress.md`, `classification-matrix.md`,
`module-flow/*.md` (Phases 1-3). This document lists every ingress point that writes/receives data
but produces no observable downstream effect (event, consumer action, cross-module read),
organized by module. Severity is High/Medium/Low based on whether the ingress point is
lifecycle-triggering (mutates durable state that other parts of the system are meant to react to).

Every row is either carried forward from Phases 1-3 (cited) or newly spot-verified this phase
(marked **[NEW]**, with the file:line evidence read directly).

---

## Severity legend

- **High** — a lifecycle-triggering write (create/update/close/cancel a domain entity, or accept
  external input) that silently produces zero downstream effect, with no error, no queue backlog,
  nothing to alert on. The system believes it "did the thing."
- **Medium** — a lifecycle-triggering write where the *primary* effect (the DB write itself) works,
  but a documented or expected secondary effect (event, notification, cross-module signal) is
  missing, and the absence is at least discoverable by an operator who looks for it (e.g. an
  explicit TODO, or a table that's provably never read).
- **Low** — read-only or infrastructure-only orphaning (dead code, a `NULL`-pattern class caught in
  the act of being a placeholder, a scheduled job that runs but does nothing beyond its own
  bookkeeping) — no lifecycle transition depends on it working.

---

## PlantSafety

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `POST api/v{version}/accidentmanagement` (Report Accident) | `PlantSafety.Api\AccidentManagement\Controllers\V1\AccidentManagementV1Controller.cs:49` → `Accident.Create` in `PlantSafety.Domain\AccidentManagement\Aggregates\Accident.cs:105`(ish) | Inserts `Accident` row via `usp_Accident_Insert`; raises `AccidentCreatedEvent`, written to Outbox | **None whatsoever.** No `AccidentCreatedConsumer` class exists anywhere (not even a stub) | **High** | Phase 3 `module-flow/plantsafety.md` §AccidentManagement |
| 2 | 6 sibling AccidentManagement lifecycle commands: `AssignInvestigator`, `RecordRootCause`, `CreateCorrectiveAction`, `CompleteCorrectiveAction`, `FileRegulatoryReport`, `CloseInvestigation` | Same controller, `AccidentManagementV1Controller.cs:83-198` | Each raises its own event (`AccidentInvestigatorAssignedEvent`, `AccidentRootCauseRecordedEvent`, `AccidentCorrectiveActionCreatedEvent`, `AccidentCorrectiveActionCompletedEvent`, `AccidentRegulatoryFilingCompletedEvent`, `AccidentInvestigationClosedEvent`) — confirmed present in `Accident.cs` via grep | **None** — no consumer class for any of the 6 | **High** — these are the investigation-lifecycle events; the entire accident investigation workflow (assign → root-cause → CAPA → regulatory filing → close) produces zero side effects beyond its own row | Phase 3 `module-flow/plantsafety.md` |
| 3 | 22 `*CreatedEvent` → `*CreatedConsumer` pairs (Culture, Dashboard, Engagement, PredictiveAnalytics, ScoreCards, Auditing, Certification, Compliance, ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar, Alert, Checklists, PPEManagement, RiskManagement, Schedules, EmergencyManagement, IncidentManagement, Observation, CompetencyManagement) | `PlantSafety.Infrastructure\**\Messaging\*CreatedConsumer.cs` (21-24 in each) | Each aggregate's `Create` raises its event → Outbox | Consumer classes exist, are real MassTransit `IConsumer<T>` implementations, but (a) `AddConsumer<T>()` is never called for any of them in `InfrastructureServiceRegistration.cs` — confirmed real RabbitMQ bus is live but has zero bound receive endpoints — and (b) even if bound, all 22 bodies are idempotency-check-then-log no-ops | **High** — genuine lifecycle-triggering creates (accidents excepted, everything else in the 22 domains) with a consumer that exists on paper but does nothing | Phase 3 `module-flow/plantsafety.md` §MassTransit correction; Phase 2 classification-matrix.md |
| 4 | **[NEW] Every non-Created lifecycle transition on the same 22+ aggregates** — `Updated`, `Closed`, `Cancelled`, `Completed`, `Assigned`, `Recorded`-style domain events | Confirmed present via grep across 26 aggregate files including `Alert.cs`, `Risk.cs`, `SafetyPolicy.cs`, `PermitWorkflow.cs`, `ChecklistCompletion.cs`, `AuditEngagement.cs`, `CultureAssessment.cs`, etc. (`AddDomainEvent(new \w+(Updated\|Closed\|Cancelled\|Deleted\|Completed\|Assigned\|Recorded)Event`) | Raises the event, writes to Outbox (same mechanism as Created) | **Zero consumers of any kind** — grepped `PlantSafety.Infrastructure` for any `*Consumer` file matching `Updated\|Deleted\|Cancelled\|Closed` in its name/type param: **zero results**. Only the 22 `Created` events got even a stub consumer; every other lifecycle transition (the majority of the write surface, by aggregate method count) has no consumer at all | **High** | **[NEW]** — spot-verified this phase via grep, not previously documented as a distinct category (Phase 3 only called this out for AccidentManagement specifically; this generalizes it solution-wide) |
| 5 | `POST api/v1/webhooks/receive` (sole external ingress) | `PlantSafety.Api\Webhooks\WebhookReceiverV1Controller.cs` → `ProcessInboundWebhookCommandHandler` | `IInboundWebhookRepository.RecordEventAsync` stores raw payload (`EventId, EventType, Source, TenantId, SiteId, OccurredAt, RawPayload`) | **None.** No outbox enqueue, no domain event, no parsing/routing of the payload after the write — despite the controller's own header comment claiming "async outbox processing" | **High** — this is the one point where an *external system* is told its data was accepted, and nothing happens to it afterward; a documentation/implementation mismatch, not just an unimplemented feature | Phase 3 `module-flow/plantsafety.md` §Webhooks |
| 6 | 130+ Analytics GET endpoints (Culture Behavior/CultureIndex/EngagementScoring/TrendModeling, Dashboard Executive/KPISnapshots/ReadModel/TrendAnalysis, Engagement, PredictiveAnalytics, Predictive ComplianceFailure/FatigueRiskModeling/IncidentPrediction/RiskForecasting, ×V1/V2) | e.g. `BehaviorV1Controller.cs:50` | N/A — read-only | Not itself orphaned as an ingress point (it's a read), but flagged because it queries projection tables that only the 5 stub `*CreatedConsumer`s (Culture/Dashboard/Engagement/PredictiveAnalytics/ScoreCards) are meant to populate, and those consumers are no-ops (finding #3) | **Medium** (symptom, not an ingress point itself) | Phase 3 `module-flow/plantsafety.md` §Analytics/Culture |
| 7 | **[NEW]** `AddPlantSafetyPolicies()` (`PlantSafety.Api\Extensions\AuthorizationPolicyExtensions.cs:49`) defines ~30 module-level authorization policies (`Analytics.Read`, `Compliance.Create`, `SafetyEvents.Delete`, etc.) | Defined, never called | The extension method is never invoked from `AddPlantSafetyAuthorization()` (`ServiceCollectionExtensions.cs:117-135`, which registers only `RequireAuthenticatedUser` and `RequireDashboardRead`) — confirmed by grepping the whole `PlantSafety` tree for `AddPlantSafetyPolicies(`: only the declaration, zero call sites | N/A — this is dead configuration code, not an ingress point per se, but is the root cause of a related authorization gap documented in `tenant-violations.md` | **Medium** (a write-adjacent authorization control that silently does nothing) | **[NEW]** — read `AuthorizationPolicyExtensions.cs` and `ServiceCollectionExtensions.cs` in full this phase |

## DataLake

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `POST api/v1/ingestion` (Ingest Raw Data) | `DataLake.Api\Controllers\v1\IngestionController.cs:20` → `IngestRawDataCommandHandler` | Inserts `IngestionJob` row via `DataLake.CreateIngestionJob` | `RawDataIngestedEvent` class exists (full audit envelope) but is **never instantiated or published anywhere** — the handler only logs and returns | **High** — this is DataLake's primary ingestion endpoint; the entire raw-zone-write step produces no signal to anything downstream (no processing trigger, no notification) | Phase 3 `module-flow/datalake.md` §Flow 1 |
| 2 | `POST api/v1/processing/raw-to-cleansed` | `ProcessingController.cs:21` → `ProcessRawToCleansedCommandHandler` | Inserts `ProcessingJob` row | `RawToCleansedCompletedEvent` class exists, never referenced outside its own definition file | **High** | Phase 3 `module-flow/datalake.md` §Flow 2 |
| 3 | `POST api/v1/processing/cleansed-to-curated` | `ProcessingController.cs:32` (pattern-inferred handler) | Presumed `ProcessingJob` row (cleansed→curated) | `CleansedToCuratedCompletedEvent` class exists, never referenced outside its own definition file (same grep as #2) | **High** (pattern-inferred, same shape as directly-verified #2) | Phase 3 `module-flow/datalake.md` §Flow 2 note |
| 4 | `POST api/v1/integration/events` (Publish Integration Event) | `IntegrationController.cs:20` → `PublishIntegrationEventCommandHandler` | Inserts integration-event record | `IntegrationEventPublishedEvent` **is actually published** via `IPublisher.Publish` — but zero `INotificationHandler<IntegrationEventPublishedEvent>` exists anywhere; published into the void | **High** — the one case where DataLake's Application layer does the "right thing" (publish) and it still dead-ends | Phase 3 `module-flow/datalake.md` §Flow 3 |
| 5 | All of DataLake's Outbox infrastructure (`OutboxProcessor`, `OutboxRepository`, `OutboxCleanupService`) | `DataLake.Infrastructure\Outbox\*.cs` | N/A — infra | Fully wired, tenant-scoped, hosted, and polling — but grepping all of `DataLake.Application` for any write to `OutboxMessage`/`IOutboxRepository` returns zero hits. Nothing in the module ever populates it | **Medium** (infra dormancy, not a specific data-loss event since nothing is ever queued to begin with — but represents real engineering effort with zero effect) | Phase 3 `module-flow/datalake.md` §Outbox section |

## Intelligence

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `POST api/v1/aiinvestigations` (Run Investigation) | `AIInvestigationsController.cs:25` → `RunInvestigationCommandHandler` | Inserts investigation row (`intelligence.usp_Investigation_Create`) | No `IPublisher`/`IIntelligenceEventPublisher` reference anywhere in the handler — no event of any kind | **High** | Phase 3 `module-flow/intelligence.md` §Flow 1 |
| 2 | `POST api/v1/datalake/ingest` (Intelligence's own DataLake subsystem) | `DataLakeController.cs:25` → `IngestDataCommandHandler` | Inserts `DataLakeRecord` row | `DataLakeIngestionCompletedIntegrationEvent` class exists, never published | **High** | Phase 3 `module-flow/intelligence.md` §Flow 2 |
| 3 | `POST api/v1/datalake/transform` | `DataLakeController.cs:46` → `TransformDataCommandHandler` | No direct DB write — delegates to `IPipelineOrchestrationService` | **Published** via `IIntelligenceEventPublisher.PublishAsync` → written to Outbox correctly → drained by `OutboxProcessor` → handed to `IOutboxPublisher`, whose **only registered implementation is `NullOutboxPublisher`**, which logs a warning and marks the message `Processed = true` without delivering it | **High** — the single most severe dead-end in the entire discovery: a working, correctly-engineered publish pipeline terminates in a stub that actively lies about delivery success | Phase 3 `module-flow/intelligence.md` §Flow 3 |
| 4 | `POST api/v1/aitraining`, `.../cancel`, `.../evaluate` | `AITrainingController.cs:24-65` → `StartTrainingJobCommandHandler`/`CancelTrainingJobCommandHandler`/`EvaluateTrainingJobCommandHandler` | Inserts/updates training-job rows | `TrainingJobStartedIntegrationEvent`/`TrainingJobCancelledIntegrationEvent` classes exist, never referenced by any handler | **High** (pattern-inferred from grep, not individually read line-by-line) | Phase 3 `module-flow/intelligence.md` §AITraining |

## Azure

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | All 24 POST/PUT/PATCH mutating Azure endpoints (restart, scale, purge, throughput/tier update, triage, disable, etc.) across 9 subdomains | e.g. `Azure\Compute.Api\Controllers\v1\ComputeController.cs:147` | ARM control-plane call (Azure's "repository" is a live outbound API call, not a DB write — see Phase 3 architectural note) | Each publishes its own domain event, consumed by exactly one same-subdomain `INotificationHandler<T>` that writes a single audit-log line and nothing else. The event's own doc comment (`ContainerAppRestartedEvent`) explicitly says cross-module fan-out "can be added in future" — never built | **Low** — this is a complete, working, intentional event lifecycle; its terminus is simply trivial (log-only), not broken. Included here for completeness per the task's ask, but Phase 3 correctly distinguishes this from a genuine dead-end | Phase 3 `module-flow/azure.md` |

## Reporting

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `POST api/v1/reports/{reportTypeCode}/generate`, `.../export` | `Reporting.Api\Controllers\v1\ReportsController.cs` → `GenerateReportCommandHandler`/`ExportReportCommandHandler` | **No persisted state change at all** — produces an in-memory PDF byte array streamed directly back in the HTTP response; no repository/DbContext dependency injected into the handler | N/A — there is no write to have a downstream effect from. Flagged here only because these are nominally "commands" (POST verb) that never touch storage — an architecture note, not an orphaning bug | **Low** | Phase 3 `module-flow/reporting.md`; `classification-matrix.md` |

## ExportImport

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `StartImportCommand`/`ImportPipelineService` and its full CSV/JSON/XML/Excel adapter chain | `ExportImport.Application\Services\Import\ImportPipelineService.cs:31`, `ExportImport.Application\Handlers\Import\StartImportCommandHandler.cs:27` | Would persist an import-operation record and publish `ImportStarted`/`ImportCompleted`/`OperationFailed` events **if reached** | **The entire module has no caller.** No `*.Api` project, no controller, no consumer, no scheduled job anywhere in the solution references `ExportImport.Application`/`ExportImport.Infrastructure` (confirmed zero hits, Phase 1 + re-confirmed Phase 3) | **High as a design gap, N/A as a live ingress point** — nothing ever ingresses here at all, so there is no invocation to "orphan," but the built capability itself is 100% unreachable | Phase 1 `inventory.md` §3/§6; Phase 3 `module-flow/exportimport.md` |

## SafetyIntegrations

| # | Ingress Point | File:Line | Writes | Downstream Effect | Severity | Basis |
|---|---|---|---|---|---|---|
| 1 | `EnterpriseGatewayInboundAdapter` (implements `IInboundAdapter`) | `SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\EnterpriseGatewayInboundAdapter.cs` | Would receive/dispatch external-system or legacy-module data via the custom `IEventBus` if invoked | **No live caller found anywhere** — no HTTP listener, no queue subscription, no scheduled poll invokes `IInboundAdapter.ReceiveAsync` | **High as a design gap, N/A as a live ingress point** — same shape as ExportImport: nothing ever calls in | Phase 1 `inventory.md` §2/§6; Phase 3 `module-flow/safetyintegrations.md` |

---

## Summary counts (this document)

| Severity | Count |
|---|---|
| High | 16 |
| Medium | 3 |
| Low | 2 |
| **Total rows** | **21** |

Note: "High as a design gap, N/A as a live ingress point" rows (ExportImport, SafetyIntegrations)
are counted once each under High above, since the underlying gap (built capability, zero callers)
is itself the finding, even though there is no live invocation to point at.
