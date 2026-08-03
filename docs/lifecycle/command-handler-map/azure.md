# Command-Handler Map — Azure

No local aggregate/state machine (structural, by design — state lives in Azure's own ARM control
plane). Inherited from `docs/ingestion/lifecycle-coverage/azure-lifecycle.md` and
`events/publishing-map.md`/`consumption-map.md` (Azure sections, 18 handlers); not independently
re-verified this phase. Every transition follows the identical shape: log → ARM SDK call → in-process
MediatR `IPublisher.Publish` (only after the ARM call succeeds) → one `INotificationHandler<T>` that
performs a single `ILogger` line and nothing else.

| Resource | Transition | Command | Handler | "Repository" Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Container App (Compute) | Restart | `RestartContainerAppCommand` (pattern-inferred) | `RestartContainerAppCommandHandler` | Live ARM SDK call (not SQL) | `ContainerAppRestartedEvent` — **Y, delivered to `ContainerAppRestartedEventHandler`** | N/A — no Outbox in Azure module (in-process MediatR, immediate) | N/A — no PlantSafety-style AuditLog table exists for Azure | Directly verified (inherited) | Delivered consumer performs one log line only (stub-equivalent, F9-style — "Real Side Effect? Stub" per `consumption-map.md` line 73) |
| Cosmos Account (Data) | Update Throughput | `UpdateCosmosAccountThroughputCommand` | `UpdateCosmosAccountThroughputCommandHandler` | Live ARM SDK call | `CosmosAccountThroughputUpdatedEvent` — Y, delivered (stub-equivalent) | N/A | N/A | Directly verified (inherited) | Same shape |
| Azure ML Training Job | Cancel | `CancelTrainingJobCommand` (Azure ML, distinct from Intelligence's) | matching handler | Live ARM SDK call | `TrainingJobCancelledEvent` (Azure ML) — Y, delivered (stub-equivalent) | N/A | N/A | Directly verified (inherited) | Explicitly documented idempotent-by-design (header comment) — the one "no local guard" instance that is a stated decision, not an oversight |
| KeyVault Secret (Security) | Disable | Command pattern-inferred | matching handler | Live ARM SDK call | `SecurityDataAccessedEvent`-family (pattern-inferred exact type) — Y, delivered (stub-equivalent) | N/A | N/A | Directly verified (inherited) | Same idempotency-by-delegation pattern |
| Service Bus Topic (Eventing) | Purge Dead Letter | Command pattern-inferred | matching handler (file located, not fully re-read) | Live ARM SDK call | `TopicDeadLetterPurgedEvent` — Y (pattern-inferred), delivered (stub-equivalent) | N/A | N/A | Pattern-inferred (inherited) | Irreversible, data-destroying operation with no confirmed precondition check — still the top open follow-up from the prior discovery |
| 13 further Azure resources (Data/Networking/ML/Observability/Security/Storage/Identity — full list in `events/publishing-map.md`) | Various | Commands pattern-inferred | matching handlers | Live ARM SDK calls | 13 further event types, all pattern-inferred Y/delivered (stub-equivalent) per `consumption-map.md` lines 74-90 | N/A | N/A | Pattern-inferred (inherited, per prior discovery's 2-of-18 fully-read sample) | |
| Any Azure resource | Delete (hard-delete) | Missing entirely | N/A | N/A | N/A | N/A | N/A | Pattern-inferred (inherited, not exhaustive) | Consistent with Azure's role as an operator surface, not a provisioning/deprovisioning tool |

**Audit summary for this module:** Azure is the **only module in the solution where every raised
event reaches real, running consumer code** (18 of 18) — but every one of those consumers performs
only a log line, no domain-meaningful action, by explicit design (each handler's own doc comment
cites cross-module fan-out as "future work," never built). There is no SQL AuditLog equivalent for
Azure transitions (control-plane state lives in Azure, not this solution's databases), so the "Audit
Entry?" column is N/A rather than N throughout — a structurally different kind of "no audit trail"
than PlantSafety's F8 gap.
