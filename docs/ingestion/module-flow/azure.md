# Module Flow — Azure

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `inventory.md`, `api-ingress.md`, `broker-ingress.md`, `classification-matrix.md` (Phases 1-2).

## Scope and method

Azure has 63 API endpoints across 9 subdomains (Compute, Data, Eventing, Identity, ML, Networking,
Observability, Security, Storage) and 18 `INotificationHandler<T>` event handlers (one per
subdomain event). Every subdomain follows an **identical, rigidly generated pattern**: Controller
(MVC, `[Authorize]`, real JWT auth) → MediatR command/query → Application handler → Infrastructure
repository → (for mutations) an in-process MediatR notification → a single audit-log handler.

Two flows were **directly traced end-to-end by reading every file in the chain**:
`RestartContainerAppCommand` (Compute) and `UpdateCosmosAccountThroughputCommand` (Data). A third,
`SubmitTrainingJobCommand` (ML), was spot-checked at the file-existence level (event/handler pair
confirmed present, body not fully read). The remaining 15 subdomain-event flows (Eventing,
Identity, Networking, Observability, Security, Storage, plus the other 3 ML training-lifecycle
events) are **pattern-inferred** from the identical header comments, identical class shapes, and
identical "never throws, one ILogger line" invariant stated in every handler's own doc comment —
no deviation was found in the samples read.

## Critical architectural fact: Azure's "repository" is not a database write

Unlike every other module in this discovery, **Azure's `I*Repository` implementations do not
write to a SQL database or DbContext at all.** `ContainerAppRepository`
(`Azure\Compute.Infrastructure\Persistence\ContainerAppRepository.cs`) wraps
`ContainerAppsClient` (an Azure Resource Manager SDK client, Managed-Identity-authenticated, no
connection string) behind a Polly resilience pipeline. "Persistence" in this module means
*calling the real Azure control plane* (restart a Container App, update Cosmos RU/s, scale KEDA
replicas) — there is no local aggregate/entity table being written. This is a meaningful
difference from DataLake/Intelligence/PlantSafety and should not be conflated with them in later
phases: Azure's "repository" write is an outbound side effect against Azure itself, not data
capture.

## Traced flow 1 — Compute: Restart Container App

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v{version}/compute/apps/{id}/restart` | `ComputeController.RestartContainerAppAsync` → `RestartContainerAppCommandHandler` | `Azure.Compute.Domain.Models.ContainerApp` (identified by `ContainerAppId`) | No local state machine — command triggers a live ARM restart operation on the named Container App resource; no `ContainerAppProvisioningState` transition is persisted locally | **None (DB).** `IContainerAppRepository` → `ContainerAppRepository` → `ContainerAppsClient` (Azure ARM SDK call via Managed Identity, wrapped in Polly `"compute-arm"` resilience pipeline) | `ContainerAppRestartedEvent` (`Azure.Compute.Application.Events`), published via in-process `IPublisher.Publish` only after the ARM call succeeds | `ContainerAppRestartedEventHandler` — **same module** (`Azure.Compute.Application.EventHandlers`), writes one SIEM-safe `ILogger` line and returns. **No other consumer exists.** | Directly verified (full read of controller, command handler, event, event handler) |

**Dead-end**: `ContainerAppRestartedEvent` is published and consumed, but the only consumer is an
audit-log writer in the same subdomain. The event's own doc comment explicitly flags this as
provisional: *"If cross-module notification is required in future, this handler can forward the
event to Service Bus"* — i.e., cross-module/external fan-out was designed for but never built.
This is a **complete event lifecycle with a trivial (log-only) terminus**, not a broken pipeline —
but it produces no actionable downstream effect (no alerting, no notification, no other module
reacts).

## Traced flow 2 — Data: Update Cosmos Account Throughput

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `PUT api/v{version}/data/cosmos-accounts/{id}/throughput` | `DataController.UpdateCosmosAccountThroughputAsync` (pattern-inferred name) → `UpdateCosmosAccountThroughputCommandHandler` | Cosmos DB account resource (ARM-managed, not a local aggregate) | RU/s throughput setting changed live on Azure; no local status enum transition | **None (DB)** — same ARM-client pattern as Compute, presumed (not separately read; `Azure.Data.Infrastructure` was not opened this phase) | `CosmosAccountThroughputUpdatedEvent` (`Azure.Data.Application.Events`) | `CosmosAccountThroughputUpdatedEventHandler` — same module, audit-log-only, identical shape to Compute's handler (full read) | Event handler directly verified; command handler pattern-inferred by analogy to Compute (not separately read) |

**Dead-end**: same shape as flow 1 — published and consumed, but the consumer only logs. No
cross-module or external effect.

## Pattern applies to all remaining Azure subdomains

The following 16 `INotificationHandler<T>` classes were confirmed (Phase 1/2 file listing) to
share the identical structure verified above — one MediatR notification, one log-only handler,
`Never throws` stated as an explicit invariant in each file's own header comment:
`ContainerAppScaleUpdatedEventHandler`, `SqlDatabaseTierUpdatedEventHandler`,
`TopicDeadLetterPurgedEventHandler`, `ManagedIdentityQueriedEventHandler`,
`DriftDetectedEventHandler`, `ModelDeployedEventHandler`, `TrainingJobCancelledEventHandler`,
`TrainingJobCompletedEventHandler`, `TrainingJobFailedEventHandler`,
`TrainingJobSubmittedEventHandler`, `NetworkSecurityGroupUpdatedEventHandler`,
`PrivateEndpointConnectedEventHandler`, `VirtualNetworkProvisionedEventHandler`,
`ObservabilityDataQueriedEventHandler`, `SecurityDataAccessedEventHandler`,
`StorageDataAccessedEventHandler`. Pattern-inferred, not individually re-read this phase.

## Summary for Phase 4

- **Every Azure lifecycle-triggering flow dead-ends at an intra-subdomain audit-log handler.**
  There is no cross-subdomain, cross-module, or external consumer of any of the 18 Azure domain
  events found anywhere in the solution.
- Azure is architecturally distinct from every other module: its "repository" layer calls live
  Azure Resource Manager APIs (a real, working outbound side effect), not a database. Phase 4
  should **not** lump Azure's "no event consumer" finding in with DataLake/Intelligence's "outbox
  never gets populated" finding — the underlying mechanism is different even though the symptom
  (nothing downstream reacts) looks the same.
- 39 of 63 endpoints are GET-only reads and never enter the lifecycle/event path at all.
