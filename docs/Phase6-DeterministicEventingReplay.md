# PHASE 6 — DETERMINISTIC EVENTING & REPLAY (PLANTSAFETY-INTERNAL SCOPE)

**Program:** PlantSafety Modernization
**Scope:** Facility, Employee, Machine, WorkflowEngine, Audit (partial) — internal to the PlantSafety modernization program repository only.
**Explicitly excluded:** Routing, Scheduling, Diagnostics (separate, unrelated repositories with no integration point of any kind — confirmed by direct search; treated as non-participants, not future integration targets). Maintenance/PredictiveMaintenance/ProductionTracking ("E2/E5/E6") — do not exist anywhere in this codebase.
**Status:** Specification only. No code changes in this phase.

---

## Scope Correction (Authoritative)

The originally requested cross-repo routing chain (Facility → WorkflowEngine → Routing → Scheduling → Diagnostics → Audit) does not correspond to anything in this codebase — no adapter, queue, or HTTP client connects PlantSafety to Routing/Scheduling/Diagnostics anywhere. Phase 6 is scoped, per explicit instruction, to **deterministic eventing, replay, rebuild, and routing inside the PlantSafety modernization program only**, using the real, already-built (but currently unused) `Shared.EventBus` infrastructure. No new event flows, no external repos, no nonexistent modules.

---

## 0. Foundational Finding: Two Disconnected Eventing Systems Exist Today

This finding governs every section below and must not be re-litigated per-section.

| | `Shared.ModuleContracts.IModuleEventGateway` | `Shared.EventBus` |
|---|---|---|
| Built in | Phase 2.7 (gateway), Phase 3.4 (`PublishIsolatedAsync`), Phase 4 (Facility/Employee/Machine wired to it) | Phase B.4 — predates the phase numbering used in this session; the older, more mature system |
| Transport | In-memory `ConcurrentDictionary<Type, List<Handler>>` | Durable SQL: `dbo.EventQueue`, `ProcessedEvents`, `EventRetryState`, `EventDeadLetterQueue` |
| Persistence | None — an event not yet consumed at publish time is lost forever | Durable — survives process restart |
| Versioning | None — `IModuleEvent` has no version field | `EventEnvelope` (V1) and `EventEnvelopeV2` coexist; V2 is additive-only over V1, with a documented `EventEnvelopeV1ToV2Adapter` |
| Correlation/Causation | `CorrelationId` passed as a side-channel parameter to `PublishIsolatedAsync`, never reaches the event object a subscriber receives; `CausationId` does not exist as a concept | `CorrelationId` and `CausationId` are first-class, required columns on `EventEnvelope`/`EventEnvelopeV2` and on `dbo.EventQueue` itself |
| Idempotency | None | `ProcessedEvents`, keyed by `(ConsumerGroup, EventId)` |
| Retry / backoff | None (a throwing subscriber is caught and logged once, never retried) | `SqlEventSubscriber` — bounded retries with exponential backoff (`EventBusOptions.ComputeBackoff`), configurable `MaxDeliveryAttempts` |
| Poison/dead-letter handling | None | `EventDeadLetterQueue`, plus a distinct `IsPoisonMessage` flag for repeated-identical-failure detection (`PoisonMessageRepeatThreshold`) |
| Currently used by Facility/Employee/Machine? | **Yes** (exclusively, since the Phase 4 domain passes) | **No** — zero references from `Program.cs` or any of the three domain services |
| Facility/Employee/Machine's own stated intent | — | Each domain event's header comment states publishing "through Shared.EventBus (as an integration event)" was always the intended real path |

**Conclusion:** Facility, Employee, and Machine publish real integration events today, but through the system with none of the durability, versioning, correlation, retry, or dead-letter properties this phase requires — while the system that already has all of those properties sits completely dormant. Phase 6 activates `Shared.EventBus` for these three domains; it does not build a third system.

---

## 1. Event Versioning Specification

### 1.1 In-Scope Events

| Module | Domain Event | Current Integration Event (`IModuleEvent`, in-memory) | Target: `EventEnvelopeV2` `EventType` | Version |
|---|---|---|---|---|
| Facility | `FacilityCreatedEvent` | `Shared.FacilityDomain.Infrastructure.Events.FacilityCreatedEvent` | `Facility.Created` | v1 |
| Facility | `FacilityUpdatedEvent` (+`FacilityUpdateKind`) | same | `Facility.Updated` | v1 |
| Facility | `FacilityHierarchyUpdatedEvent` (+`FacilityHierarchyChangeKind`) | same | `Facility.HierarchyUpdated` | v1 |
| Facility | `FacilityActivatedEvent`/`FacilityShutdownEvent`/`FacilityReopenedEvent`/`FacilityDecommissionedEvent` | consolidated into `FacilityLifecycleChangedEvent` | `Facility.LifecycleChanged` | v1 |
| Employee | `EmployeeCreatedEvent` | same | `Employee.Created` | v1 |
| Employee | `EmployeeUpdatedEvent` (+`EmployeeUpdateKind`) | same | `Employee.Updated` | v1 |
| Employee | `EmployeeTrainingUpdatedEvent` | same | `Employee.TrainingUpdated` | v1 |
| Employee | `EmployeeCertificationUpdatedEvent` | same | `Employee.CertificationUpdated` | v1 |
| Machine | `MachineCreatedEvent` | same | `Machine.Created` | v1 |
| Machine | `MachineUpdatedEvent` (+`MachineUpdateKind`) | same | `Machine.Updated` | v1 |
| Machine | `MachineMaintenanceUpdatedEvent` | same | `Machine.MaintenanceUpdated` | v1 |
| Machine | `MachineFailureRecordedEvent` | same | `Machine.FailureRecorded` | v1 |
| WorkflowEngine | *(none exist)* | — | `WorkflowEngine.DecisionEvaluated` | v1 (new — see §1.4) |
| Audit | *(none — read path only, see §1.5)* | — | N/A | N/A |

All events start at **v1**: none of them have ever been published through a versioned envelope before, so there is no legacy shape to reconcile against. This is a deliberate simplification available only because activation happens before any real subscriber depends on today's ad hoc `IModuleEvent` shapes.

### 1.2 Versioning Rules

1. **Schema version is a required field on every envelope**, carried on `EventEnvelopeV2.SchemaVersion` (already exists) — not inferred from `EventType` string parsing.
2. **Backward compatibility**: a schema version increment (v1 → v2) may only be *additive* — new nullable/optional fields. No field may be removed or have its type changed within the same `EventType`. This mirrors the existing, already-followed `EventEnvelope` → `EventEnvelopeV2` precedent exactly (V2 added nullable enrichment fields; V1 consumers were untouched).
3. **Forward compatibility**: a consumer registered for schema version *N* must ignore fields it does not recognize on version *N+1* payloads rather than fail deserialization. `System.Text.Json`'s default unknown-property tolerance satisfies this without extra code, provided payload DTOs are not marked to reject unknown members.
4. **Deprecation rule**: an `EventType` is never silently repurposed. A breaking shape change requires a **new** `EventType` string (e.g. `Facility.Created` → `Facility.CreatedV2`), not a version bump on the existing type. This matches the existing legacy convention already visible in `Shared.EventBus.Abstractions.PlantSafety` (`AccidentCreatedIntegrationEvent` vs. `AccidentCreatedIntegrationEventV2` as distinct types, not one type with a version switch).
5. **Required metadata fields on every event, no exceptions**: `EventId` (Guid), `EventType` (string, `{Module}.{Name}` format), `SchemaVersion` (int), `CorrelationId` (Guid), `CausationId` (Guid), `OccurredAtUtc` (DateTime, from `IClock`, never `DateTime.UtcNow`), `SourceModule` (string). All six already exist on `EventEnvelopeV2` today — none need to be added to the envelope type. What needs to change is that Facility/Employee/Machine's publish call sites populate all six, which they do not today (their in-memory `IModuleEvent` publishes only `SourceModule` and `OccurredAtUtc`).

### 1.3 Events That Must Split or Merge

**No split is required.** Facility's existing collapse of four lifecycle domain events into one `FacilityLifecycleChangedEvent` integration event (established in the Phase 4 Facility pass, ratified in the Phase 5 spec §7.3) is preserved as-is — it already has one consistent payload shape (`FacilityId`, `NewStatus`, optional `Reason`) and needs no further change for versioning purposes.

### 1.4 New Event Required: `WorkflowEngine.DecisionEvaluated`

`Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate` is a pure function with no side effects and no aggregate — it has never raised a domain event because it has no state to report having changed. For Facility/Employee/Machine events to *route into* WorkflowEngine deterministically (§4), WorkflowEngine's evaluation itself must become an observable fact once triggered by an inbound event, not only a synchronous HTTP response. `WorkflowEngine.DecisionEvaluated` (payload: the `WorkflowContext` that was evaluated, the resulting `WorkflowDecision`, and the `EventId` of the event that triggered the evaluation as `CausationId`) is the one new event this phase requires — it is a direct consequence of wiring real routing, not scope creep.

### 1.5 Audit Is Not a Publish Target

`Shared.AuditLogging` is read-only: `IAuditLogRepository.SearchAsync` is its entire public surface. The underlying `dbo.AuditLog` table is written exclusively by stored procedures as a side effect of SQL writes across the eight module databases (per `AuditLogEntry`'s own header comment) — never by consuming a published event. Section 4's "WorkflowEngine → Audit" routing is therefore **not** "WorkflowEngine publishes an event, Audit consumes it" — see §4.4 for what it actually means.

---

## 2. Replay Semantics Specification

### 2.1 State Reconstruction Per Module

| Module | Reconstructs from | Replay mechanism |
|---|---|---|
| Facility | `FacilityCreatedEvent`, `FacilityUpdatedEvent`, `FacilityHierarchyUpdatedEvent`, `FacilityLifecycleChangedEvent`, in `OccurredAtUtc` order | Fold each event into a `FacilityState`, starting from `FacilityAggregate.CreateFacility`'s effect, applying each subsequent event's implied `with`-mutation in order |
| Employee | `EmployeeCreatedEvent`, `EmployeeUpdatedEvent`, `EmployeeTrainingUpdatedEvent`, `EmployeeCertificationUpdatedEvent` | Same fold pattern |
| Machine | `MachineCreatedEvent`, `MachineUpdatedEvent`, `MachineMaintenanceUpdatedEvent`, `MachineFailureRecordedEvent` | Same fold pattern |
| WorkflowEngine | *(stateless — nothing to replay)* | N/A. `WorkflowEngine.DecisionEvaluated` is an observability record of a computation, not state to be reconstructed; replaying it means re-running `WorkflowEngine.Evaluate` against the same recorded `WorkflowContext`, which is trivially deterministic (pure function) and needs no event-fold machinery at all |
| Audit | *(read-only; nothing to replay — it is itself the audit trail of everything else)* | N/A |

### 2.2 Replay Ordering Rules

1. Events replay strictly in `OccurredAtUtc` order **within one aggregate instance** (one `FacilityId`, one `EmployeeId`, one `MachineId`). Cross-instance ordering (Facility A's events vs. Facility B's events) is irrelevant — aggregates are independent and never share mutable state.
2. `EventEnvelopeV2` does not carry a monotonic sequence number today (only a UTC timestamp) — since `IClock` is deterministic and `SystemClock`/`FixedClock` guarantee non-decreasing timestamps within a process, `OccurredAtUtc` is sufficient for ordering within this program's current deterministic-clock discipline. If two events for the same aggregate ever share an identical timestamp (only possible under `FixedClock` in a test), the tie-break is publish order as recorded in `dbo.EventQueue`'s identity column, not `OccurredAtUtc` alone.
3. Replay must apply events **one aggregate instance at a time, start to finish**, never interleaved — this is a direct consequence of §2.1's fold pattern requiring the previous fold's output as the next event's input.

### 2.3 Replay Window Rules

- **Full replay**: every event for a given aggregate ID, from its `Created` event onward. Used for rebuild (§3).
- **Bounded replay**: every event for a given aggregate ID between two `OccurredAtUtc` timestamps (e.g., "what did this facility look like as of last Tuesday"). Requires the fold to start from the last snapshot at-or-before the window start (§3.3), not always from `Created`.
- **Partial replay** (a single event type only, e.g. "every `MachineFailureRecordedEvent` ever raised") is a **query**, not a state reconstruction — it does not fold into an aggregate and is out of scope for this section; it is an ordinary `dbo.EventQueue`/processed-event SQL query.

### 2.4 Replay Failure Handling

| Failure | Handling |
|---|---|
| Invalid event (fails `EventEnvelopeV2` deserialization) | Replay halts for that aggregate instance and reports the failure with the offending `EventId` — it does not skip and continue, since skipping would silently produce an incorrect reconstructed state. |
| Missing event (a gap in the expected sequence — e.g. an `Updated` event with no preceding `Created`) | Replay halts immediately. An aggregate's fold has no valid starting state without its `Created` event; this is not recoverable at replay time and must be flagged as a data-integrity incident, not silently worked around. |
| Corrupted event (deserializes successfully but violates a domain invariant when applied — e.g. a `FacilityUpdatedEvent` implying a transition `FacilityLifecycleStatus` never permits) | Replay halts and reports the specific invariant violated, using the same guard-clause logic already present in the aggregate (§7.1's centralized `Guard` helper, once introduced, is the single source of truth both live commands and replay validate against). |

In all three cases: **replay never silently produces a best-effort state.** A halted replay is a reported failure, not a partial result presented as complete.

### 2.5 Replay Determinism Guarantees

Given the same ordered event sequence, replay produces **byte-identical** resulting state, every time, on any machine. This holds today because:
- Every aggregate mutation is already a pure function of `(previous state, event payload, nowUtc)` — no aggregate method reads ambient state.
- `nowUtc` is itself embedded in the event (`OccurredAtUtc`), not re-derived from the wall clock during replay — replay never calls `IClock.UtcNow` for anything; it only ever reads timestamps already recorded on events.
- No aggregate mutation involves randomness. (If one ever needs to — none currently do — the seed would need to be captured on the triggering event, following `Shared.Deterministic`'s existing `IRandomProvider`/`SeededRandomProvider` pattern.)

---

## 3. Rebuild Semantics Specification

### 3.1 Rebuild Procedure Per Module

Rebuild ("reconstruct this aggregate's current state from nothing but its event history") is full replay (§2.3) with no snapshot shortcut, run for every known aggregate ID. It is identical to replay in mechanism; the distinction is scope (one instance being replayed for a query vs. every instance being rebuilt to repopulate a store).

### 3.2 Rebuild Checkpoints

A checkpoint is recorded after every N events replayed for a given aggregate (N configurable, default 100 — matching the existing `SqlEventSubscriber.ProcessDueEventsAsync`'s default `maxBatchSize` of 50, doubled since checkpointing is cheaper than dispatch). A checkpoint records: aggregate ID, last-applied `EventId`, last-applied `OccurredAtUtc`, and the folded state as of that point (JSON-serialized `FacilityState`/`EmployeeState`/`MachineState` — already `System.Text.Json`-serializable per Phase 4's domain-modeling rule). A rebuild interrupted mid-run resumes from the last checkpoint, not from zero.

### 3.3 Rebuild Snapshot Rules

A snapshot is a checkpoint promoted to long-term storage (vs. an in-flight rebuild's transient checkpoint). Rules:
1. A snapshot is only ever a **cache of a fold result**, never a source of truth — the event log remains authoritative. Deleting every snapshot must never lose information, only rebuild time.
2. A snapshot is taken automatically after every successful full rebuild, and additionally after every 500 events applied to a single aggregate during normal (non-rebuild) operation, to bound worst-case rebuild time for high-churn aggregates.
3. A snapshot is invalidated (not deleted, marked stale) the moment schema version rules (§1.2) change in a way that isn't purely additive for that aggregate's event types — forcing a full rebuild rather than resuming from a snapshot computed under different versioning assumptions.

### 3.4 Rebuild Performance Constraints

Given the program's stated 25-workstation, on-prem, single-tenant scale: no aggregate in this program (Facility, Employee, or Machine) is expected to accumulate more than a few thousand events over the system's operational lifetime. A full rebuild of every aggregate in every module is therefore bounded by total event count, not by any per-aggregate pagination concern — `SqlEventSubscriber`'s existing `maxBatchSize` batching is sufficient without new infrastructure. No additional performance work is specified here because none is evidenced as necessary at this scale.

### 3.5 Rebuild Validation Rules

After a full rebuild, the rebuilt state for every aggregate is compared against the corresponding row in the live in-memory store (`IFacilityStore`/`IEmployeeStore`/`IMachineStore`) if one exists. A mismatch is a data-integrity finding to be reported, not auto-corrected — auto-correcting would mean guessing which of the two disagreeing states is right, which this phase's determinism guarantee (§2.5) says should never be ambiguous in the first place. A mismatch, if it ever occurs, indicates the live store was mutated by something other than a properly-published, properly-replayed event — itself a bug to find and fix, not a state to silently reconcile.

---

## 4. Cross-Module Event Routing Specification (PlantSafety-Internal Only)

### 4.1 Routes In Scope

| From | To | Trigger | Consumer action |
|---|---|---|---|
| Facility | WorkflowEngine | `FacilityLifecycleChangedEvent` (NewStatus = ShutDown), `FacilityHierarchyUpdatedEvent` | Build a `WorkflowContext` from the event's implied hazard/severity context and call `WorkflowEngine.Evaluate`, publishing `WorkflowEngine.DecisionEvaluated` with `CausationId` = the triggering event's `EventId` |
| Employee | WorkflowEngine | `EmployeeUpdatedEvent` (Kind = InjuryProfile) | Same pattern |
| Machine | WorkflowEngine | `MachineFailureRecordedEvent` | Same pattern — this is the literal, direct fulfillment of that event's own header comment ("downstream hazard propagation... can react without re-deriving it"), which today has no subscriber at all |
| WorkflowEngine | Audit | *(not an event-publish relationship — see §4.4)* | — |

### 4.2 Routing Metadata Requirements

Every routed event must carry, at minimum, the six required fields from §1.2, plus: the routing consumer (`SqlEventSubscriber.ConsumerGroup`) that processed it, recorded in `ProcessedEvents` for idempotency, and the resulting `WorkflowEngine.DecisionEvaluated` event's `CausationId` set to the *routed* event's `EventId` — never a synthetic or newly-generated ID — so a single Facility shutdown can be traced forward through exactly one `WorkflowEngine.DecisionEvaluated` record with an unbroken causation chain.

### 4.3 Routing Failure Handling

Routing failure handling is `SqlEventSubscriber`'s existing, already-built retry/backoff/dead-letter behavior (§0's comparison table) — this phase does not invent new failure handling, it activates what already exists for these three new consumer groups (`WorkflowEngine.FacilityConsumer`, `WorkflowEngine.EmployeeConsumer`, `WorkflowEngine.MachineConsumer`).

### 4.4 What "WorkflowEngine → Audit" Actually Means

There is no `IAuditLogWriter` anywhere in this program — `Shared.AuditLogging` is read-only by design (§1.5), and its write path is exclusively the stored-procedure-level `dbo.AuditLog` insert pattern already used across every module's real database writes. "WorkflowEngine → Audit" routing, correctly scoped, means: `WorkflowEngine.DecisionEvaluated` is a row in `dbo.EventQueue`/its processed-event history, and that history **is** the audit trail for workflow decisions — no separate publish-to-Audit step exists or should be built. This corrects the original prompt's implied "WorkflowEngine publishes, Audit consumes" framing to what the actual write-once audit architecture already established in Phase 5 (§4, `IAuditLogRepository`) supports.

### 4.5 Routing Determinism Guarantee

Given the same ordered set of inbound Facility/Employee/Machine events, the routing layer produces the same set of `WorkflowEngine.DecisionEvaluated` events with the same `WorkflowDecision` payloads, every time — a direct consequence of `WorkflowEngine.Evaluate` already being a pure function (verified in the Phase 4 WorkflowEngine review) and the routing layer introducing no additional non-determinism (no wall-clock reads, no randomness) beyond what §2.5 already guarantees for the triggering events themselves.

---

## 5. Deterministic State Reconstruction Specification

### 5.1 Ordering Guarantees

Restates §2.2 as a cross-cutting guarantee: per-aggregate-instance `OccurredAtUtc` order is the only ordering guarantee this program makes or needs, given single-tenant, single-process, on-prem deployment with no distributed clock skew to reconcile.

### 5.2 Idempotency Rules

Every consumer (the three new WorkflowEngine routing consumers, and any future one) **must** be safe to invoke more than once for the same `EventId` — `SqlEventSubscriber`'s `ProcessedEvents` table, keyed by `(ConsumerGroup, EventId)`, is the single mechanism enforcing this; no consumer may implement its own separate idempotency check. `IEventHandlerV2<TPayload>.HandleAsync`'s own doc comment already states this requirement ("MUST be idempotent -- may be invoked more than once for the same EventId under at-least-once delivery") — this phase does not change that contract, only finally exercises it.

### 5.3 Conflict Resolution Rules

Two events for the same aggregate instance never conflict in this architecture, because every mutation is a full-state `with`-replacement recorded as one event, applied in one deterministic order (§2.2) — there is no concept of two concurrent writers racing to mutate the same aggregate that this program's single-process, `Scoped` (per-request) repository pattern would allow. If concurrent HTTP requests target the same `FacilityId` simultaneously, standard optimistic-concurrency-via-last-write-wins already applies at the in-memory store level (`IFacilityStore.Save` overwrites); this phase does not change that and does not need to, since event *replay* is a downstream, sequential process reading an already-ordered log, not a concurrent-writer scenario.

### 5.4 Cross-Module Consistency Rules

Facility, Employee, Machine, and WorkflowEngine each own their own aggregate state independently — there is no shared/joint state across modules requiring cross-module transactional consistency. The routing relationships in §4 are eventually-consistent by design (a `MachineFailureRecordedEvent` is durably queued in `dbo.EventQueue` before `WorkflowEngine.Evaluate` runs against it) — this is consistent with, not a departure from, the deferred-consumption pattern `PublishIsolatedAsync` already established in Phase 3.4 (a publish failure must never roll back the triggering command).

---

## 6. Phase 6 Output: Unified Deterministic Eventing Specification

### 6.1 Consolidated Ruleset

| Area | Rule |
|---|---|
| **Transport** | All Facility/Employee/Machine/WorkflowEngine integration events migrate from `IModuleEventGateway` (in-memory, Phase 4) to `Shared.EventBus`'s `IEventPublisherV2`/`EventEnvelopeV2` (durable, already built, currently unused). `IModuleEventGateway` is not deleted — nothing else in this program depends on removing it — but no new publish call sites should be added to it once this phase's migration lands. |
| **Versioning** | Every event starts at `SchemaVersion = 1`. Increments are additive-only. A breaking change is a new `EventType` string, never a version bump that changes an existing type's required fields. |
| **Metadata** | `EventId`, `EventType`, `SchemaVersion`, `CorrelationId`, `CausationId`, `SourceModule`, `OccurredAtUtc` are required on every event, with no exceptions, sourced from `ICorrelationContext`/`IClock`, never generated ad hoc at the publish call site. |
| **Replay** | Per-aggregate-instance, `OccurredAtUtc`-ordered fold from `Created` onward (or from the nearest valid snapshot, §3.3). Any invalid, missing, or corrupted event halts replay with a reported failure — never a silent partial result. |
| **Rebuild** | Full replay for every known aggregate ID, checkpointed every 100 events, snapshotted after every successful rebuild and every 500 live events. Rebuilt state is validated against the live store, with mismatches reported, never auto-corrected. |
| **Routing** | Exactly four routes exist in this program's real scope: Facility → WorkflowEngine, Employee → WorkflowEngine, Machine → WorkflowEngine (all three via new `SqlEventSubscriber` consumer groups), and WorkflowEngine's own decisions recorded as their own auditable event stream (not a separate "→ Audit" publish step). No other routes exist or are specified. |
| **Determinism** | Guaranteed end-to-end because every link in the chain — aggregate mutation, `WorkflowEngine.Evaluate`, replay's fold — is already a pure function of explicit, recorded inputs (state, event payload, `nowUtc`), with no ambient wall-clock or randomness access anywhere in the path. |

### 6.2 Explicitly Not Specified (Out of Scope, Not Forgotten)

- Routing, Scheduling, Diagnostics: no integration point exists; not addressed by this phase and not a future target implied by this document.
- Maintenance/PredictiveMaintenance/ProductionTracking ("E2/E5/E6"): do not exist in this codebase.
- Multi-tenant, multi-country, or cloud eventing concerns: architecture is on-prem, single-tenant; `EventEnvelopeV2`'s existing `TenantId` field is a legacy carryover from the codebase `Shared.EventBus` was extracted from and is not activated or populated meaningfully by this phase — it is not a multi-tenancy feature being introduced here.
- Audit as a *write* target: `Shared.AuditLogging` remains read-only; this phase does not add a publish-to-Audit code path.

### 6.3 Readiness for Phase 7

This specification is implementation-ready once one product/engineering decision is made: **whether `Shared.EventBus`'s `EventBusOptions.ConnectionString` targets the same database as `Shared.SqlGateway`'s `Shared:SqlGateway:ConnectionString` (Phase 4.2), or a dedicated event-store database** — mirroring the exact multi-connection-string question already flagged, unresolved, in the Phase 5 spec (§7.7) for `AuditLog`'s eventual `ISqlGateway` migration. Everything else in this document — event inventory, versioning rules, replay/rebuild semantics, the four in-scope routes, and the consolidated ruleset — requires no further product input and is ready for Phase 7 implementation.
