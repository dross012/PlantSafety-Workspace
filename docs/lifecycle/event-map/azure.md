# Event Map — Azure (9 subdomains)

No local aggregate/state machine (structural, by design). Inherited from
`docs/ingestion/events/publishing-map.md`/`consumption-map.md` (Azure sections, 18 handlers) and
`command-handler-map/azure.md`; not independently re-verified this phase at the source-file level.
Every transition follows the identical shape: log → live ARM SDK call → in-process MediatR
`IPublisher.Publish` (only after the ARM call succeeds) → one `INotificationHandler<T>` that
performs a single `ILogger` line.

| Resource | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Container App (Compute) | Restart | Y | `ContainerAppRestartedEvent` | Unconfirmed — event body not opened this phase | **Y — delivered to `ContainerAppRestartedEventHandler`**, but the handler performs only a log line (Stub-equivalent per `consumption-map.md` line 73) | Directly verified (delivery path, inherited); payload not re-opened | The delivery mechanism works end-to-end here — this is the one category where "Consumer Exists & Registered?" is genuinely **Y**, not N — but the consumer's own action is trivial by design (its doc comment cites cross-module fan-out as "future work," never built) |
| Cosmos Account (Data) | Update Throughput | Y | `CosmosAccountThroughputUpdatedEvent` | Unconfirmed | **Y**, delivered (stub-equivalent) | Directly verified (delivery path, inherited) | Same shape |
| SQL Database (Data) | Update Tier | Y (pattern-inferred) | `SqlDatabaseTierUpdatedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | |
| Service Bus Topic (Eventing) | Purge Dead Letter | Y (pattern-inferred) | `TopicDeadLetterPurgedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | Irreversible, data-destroying operation with no confirmed precondition check (inherited) — still the top open follow-up regardless of payload/delivery status |
| Managed Identity (Identity) | Query | Y (pattern-inferred) | `ManagedIdentityQueriedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | |
| ML Model (ML) | Drift Detected / Deploy / Cancel/Complete/Fail/Submit Training Job (Azure ML, distinct from Intelligence's) | Y (pattern-inferred, 6 event types) | `DriftDetectedEvent`, `ModelDeployedEvent`, `TrainingJobCancelledEvent`/`CompletedEvent`/`FailedEvent`/`SubmittedEvent` (Azure ML) | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | `TrainingJobCancelledEventHandler` explicitly documented idempotent-by-design (header comment) — the one "no local guard" instance that is a stated decision, not an oversight |
| Networking (NSG, Private Endpoint, VNet) | Update / Connect / Provision (3 event types) | Y (pattern-inferred) | `NetworkSecurityGroupUpdatedEvent`, `PrivateEndpointConnectedEvent`, `VirtualNetworkProvisionedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | |
| Observability | Query Data | Y (pattern-inferred) | `ObservabilityDataQueriedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | |
| Security (KeyVault) | Disable / Access Data (2 event types) | Y (pattern-inferred) | `SecurityDataAccessedEvent`-family | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | Same idempotency-by-delegation pattern as ML |
| Storage | Access Data | Y (pattern-inferred) | `StorageDataAccessedEvent` | Unconfirmed | **Y** (pattern-inferred), delivered (stub-equivalent) | Pattern-inferred (inherited) | |
| Any Azure resource | Delete (hard-delete) | Missing entirely | N/A | N/A | N/A | Pattern-inferred (inherited, not exhaustive) | Consistent with Azure's role as an operator surface |

**Duplicate-publish check (F17):** not individually re-verified this phase; each transition's own
doc comment describes a single `IPublisher.Publish` call after a single ARM SDK call succeeds — no
evidence of a double-publish pattern in the 2 handlers fully read by the prior discovery
(`ContainerAppRestartedEventHandler`, `CosmosAccountThroughputUpdatedEventHandler`).

**Module-level summary — the one module where "Consumer Exists & Registered?" is genuinely Y:**
all 18 Azure events reach real, running consumer code (F9's own framing) — the only category in the
entire solution where this phase can answer "yes, a consumer is registered and would fire" without
qualification. However, every one of those consumers performs only a single log line, no
domain-meaningful action, by explicit design. Event payloads were not individually re-opened this
phase (flagged as a follow-up); given the terminal action is a log line regardless of payload
richness, a payload-completeness defect here would have essentially no practical consequence today,
unlike every PlantSafety/DataLake/Intelligence event where a future real consumer would need the
fields this phase's F15 finding says are missing.
