# PHASE 5 — ENTERPRISE DOMAIN HARDENING & CROSS-MODULE CONTRACT CONSOLIDATION

**Program:** PlantSafety Modernization
**Scope:** Facility, Employee, Machine, WorkflowEngine, Shared.SafetyData, Shared.SqlGateway, Shared.AuditLogging (fully audited); AI.\*/Enterprise.\* engines (structurally validated only, per standing deferral to event-driven orchestration rework)
**Explicitly excluded:** Routing, Scheduling, Diagnostics (live in separate, unrelated git repositories — not part of this program); Maintenance/PredictiveMaintenance/ProductionTracking ("E2/E5/E6") — not found anywhere in this codebase
**Status:** Specification only. No code changes in this phase.

---

## Scope Note

This audit covers only modules independently verified to exist within the modernization program repository (the tree containing `Shared/`, `UI/`, `Enterprise/`, `AI/`). `D:\PlantSafety\source` also contains several unrelated, independently version-controlled repositories (`Diagnostics/`, `Intelligence/`, `PlantSafety/`, `Reporting/`, `DataLake/`, `ExportImport/`, `SafetyIntegrations/`, `SafetyTooling/`, `Azure/`) that are not part of this program and are excluded here. No "Enterprise Rule Handbook" document exists anywhere in this program's repository; Section 6 is written against the enums and rule objects actually present in the code, not against an external document that could not be located.

---

## 1. Domain Contract Audit

### 1.1 Domain Model Inventory

| Module | Aggregate | State (immutable record) | Strong ID | Value Objects | Lifecycle Status |
|---|---|---|---|---|---|
| Facility | `FacilityAggregate` | `FacilityState` | `FacilityId` | `FacilityIndustry`, `FacilityLocation`, `FacilityHazardProfile`, `FacilityRegulatoryProfile`, `FacilityProgramProfile`, `FacilityHierarchy` (+ `BuildingId`/`AreaId`/`ZoneId`) | `FacilityLifecycleStatus` (PendingActivation → Active → ShutDown ⇄ Active → Decommissioned) |
| Employee | `EmployeeAggregate` | `EmployeeState` | `EmployeeId` | `EmployeeTrainingProfile`, `EmployeeCertification`, `EmployeeExposureRecord`, `EmployeeInjuryProfile`, `EmployeeSafetyScore` | **None** |
| Machine | `MachineAggregate` | `MachineState` | `MachineId` | `MachineLocation`, `MachineMaintenanceProfile`, `MachineFailureRecord`, `MachineSafetyScore` | **None** |
| WorkflowEngine | *(stateless — pure function)* | `WorkflowContext` / `WorkflowDecision` | — | `RequiredAction`, `SopTrigger`, `PermitTrigger`, `EvacuationTrigger`, `NotificationRouting` | N/A |
| Shared.SafetyData | *(read-only projection, no aggregate)* | `HazardSummaryEntry`, `CorrectiveActionEntry`, `SafetyComplianceEntry` | — | `SafetySearchCriteria`, `SafetySearchResult<T>` | N/A |
| Shared.AuditLogging | *(read-only projection, no aggregate)* | `AuditLogEntry` | — | `AuditLogSearchCriteria`, `AuditLogSearchResult` | N/A |

### 1.2 Cross-Cutting Model Rule Compliance

Verified against the Phase 4 domain-modeling rules (immutability, deterministic constructors, value objects, no wall-clock dependency, JSON-serializable):

| Rule | Facility | Employee | Machine | Notes |
|---|---|---|---|---|
| Immutable state (`sealed record`, `with`-expressions) | ✅ | ✅ | ✅ | Consistent across all three. |
| Aggregate never calls a repository | ✅ | ✅ | ✅ | Verified — no repository references inside any aggregate. |
| No wall-clock dependency inside the aggregate | ✅ | ✅ | ✅ | Every mutating method takes `DateTime nowUtc` as an explicit parameter; the wall-clock leak that existed was in the *service* layer, fixed in the Phase 4 domain passes (see §3.4). |
| Validation at construction (`ArgumentException` on required strings) | ✅ (Name, ShutdownReason, DecommissionReason) | ✅ (Name, InjuryDescription) | ✅ (Name, FailureDescription) | Pattern is identical across all three but implemented as **duplicated inline guard clauses**, not a shared helper — see §5.2. |

### 1.3 Findings — Inconsistencies

**F1-01 — Lifecycle status exists only for Facility.**
Facility has an explicit, guarded `FacilityLifecycleStatus` state machine (PendingActivation/Active/ShutDown/Decommissioned) with dedicated transition methods. Employee and Machine have **no equivalent concept** — there is no way to represent "this employee has left the company" or "this machine has been retired/decommissioned" in either aggregate today. An employee or machine, once created, exists forever in an undifferentiated active state. This is a real domain-completeness gap, not a naming issue — it needs a product decision (does Employee/Machine need a lifecycle?) before it can be fixed in code.

**F1-02 — Casing inconsistency: `ShutDown` vs `Shutdown`.**
`FacilityLifecycleStatus.ShutDown` (capital "D") does not match the casing used everywhere else for the same concept: `FacilityAggregate.ShutdownFacility(...)`, `FacilityShutdownEvent`, `IFacilityService.ShutdownFacilityAsync(...)`. Every other member of `FacilityLifecycleStatus` (`PendingActivation`, `Active`, `Decommissioned`) is a single readable word; `ShutDown` is the only compound-cased one. This is cosmetic but real, and the kind of thing that causes copy-paste bugs (a future author typing `FacilityLifecycleStatus.Shutdown` will get a compile error, not a subtle bug — low severity, but worth normalizing).

**F1-03 — SafetyScore exists for Employee and Machine, not for Facility.**
Both `EmployeeSafetyScore` and `MachineSafetyScore` are computed and recomputed after every mutating command (`RecomputeSafetyScore`, raising an `{X}UpdatedEvent(Kind=SafetyScore)`). Facility has no equivalent aggregate-level score. This may be intentional (a facility's "safety score" might legitimately be a derived/computed view over its employees/machines/hazards rather than aggregate-owned state) — flagged for a product decision, not asserted as a defect.

**F1-04 — `*Kind` vs `*Type` suffix convention is consistent but implicit.**
Every enum that discriminates a domain **event/change** is suffixed `*Kind` (`FacilityUpdateKind`, `EmployeeUpdateKind`, `MachineUpdateKind`, `FacilityHierarchyChangeKind`). Every enum that classifies an **entity's category** is suffixed `*Type` (`HazardType`, `MachineType`). This convention already holds 100% of the time across the audited modules but is nowhere written down — codified explicitly in §7.

---

## 2. Command/Query Contract Consolidation

### 2.1 Inventory

No module uses formal `ICommand`/`IQuery` marker types or a mediator/handler pipeline. Every module exposes one `I{Entity}Service` interface whose async methods serve as the de facto command/query surface, invoked directly by a thin controller.

| Module | Commands (state-changing) | Queries (read-only) |
|---|---|---|
| Facility | `CreateFacilityAsync`, `UpdateFacilityMetadataAsync`, `UpdateHazardProfileAsync`, `UpdateRegulatoryProfileAsync`, `UpdateProgramProfileAsync`, `AddBuildingAsync`, `AddAreaAsync`, `AddZoneAsync`, `UpdateHierarchyAsync`, `ActivateFacilityAsync`, `ShutdownFacilityAsync`, `ReopenFacilityAsync`, `DecommissionFacilityAsync` | `GetFacilityAsync`, `GetAllFacilitiesAsync` |
| Employee | `CreateEmployeeAsync`, `UpdateEmployeeMetadataAsync`, `UpdateTrainingProfileAsync`, `UpdateCertificationsAsync`, `RecordExposureAsync`, `RecordInjuryAsync` | `GetEmployeeAsync`, `GetAllEmployeesAsync` |
| Machine | `CreateMachineAsync`, `UpdateMachineMetadataAsync`, `UpdateMaintenanceProfileAsync`, `RecordFailureAsync` | `GetMachineAsync`, `GetAllMachinesAsync` |
| SafetyData | — (read-only module) | `GetHazardSummaryAsync`, `GetCorrectiveActionsAsync`, `GetSafetyComplianceAsync` |
| AuditLogging | — (read-only module) | `SearchAsync` |

### 2.2 Findings

**F2-01 — `Update*` vs `Record*` is a real, valuable, but undocumented distinction.**
Employee and Machine use `Record*Async` for operations that append an immutable historical entry (`RecordExposureAsync`, `RecordInjuryAsync`, `RecordFailureAsync`) and `Update*Async` for operations that wholesale-replace current state (`UpdateTrainingProfileAsync`, `UpdateCertificationsAsync`, `UpdateMaintenanceProfileAsync`). Facility has no `Record*` methods at all — every Facility mutation is a replacement (`Update*`/`Add*`). This split is semantically correct (`Record` = append-only fact, `Update` = value replacement) and should be codified as a naming rule (§7), not collapsed.

**F2-02 — No leakage of internal/SQL details in service signatures.**
Verified: no method on any `I{Entity}Service` accepts or returns a SQL/Dapper/EF type, connection string, or raw row projection. All parameters are primitives or the module's own value objects. This is compliant and should be preserved as a hard rule going forward.

**F2-03 — Validation is deferred entirely to the aggregate, not the command surface.**
No service method validates its own arguments beyond null-checking via the aggregate; a `Create*Async("", ...)` call reaches `FacilityAggregate.CreateFacility` before the `ArgumentException` fires. This is consistent across all three modules and consistent with Phase 4's "Handler Rules" (validation happens once, at the point of truth), but it means a command's own validation is entirely implicit — there is no `{Command}.Validate()` method anywhere, because there are no command objects. Acceptable given the current architecture; would need to change if formal Command objects are introduced later.

---

## 3. Event Model Hardening

### 3.1 Inventory

| Module | Domain Events (Abstractions) | Integration Events (Infrastructure, `IModuleEvent`) | Mapping |
|---|---|---|---|
| Facility | `FacilityCreatedEvent`, `FacilityUpdatedEvent` (+`FacilityUpdateKind`), `FacilityHierarchyUpdatedEvent` (+`FacilityHierarchyChangeKind`), `FacilityActivatedEvent`, `FacilityShutdownEvent`, `FacilityReopenedEvent`, `FacilityDecommissionedEvent` (7 types) | `FacilityCreatedEvent`, `FacilityUpdatedEvent`, `FacilityHierarchyUpdatedEvent`, `FacilityLifecycleChangedEvent` (4 types) | **Many-to-one**: the 4 lifecycle domain events collapse into 1 integration event, discriminated by a `NewStatus` string. |
| Employee | `EmployeeCreatedEvent`, `EmployeeUpdatedEvent` (+`EmployeeUpdateKind`), `EmployeeTrainingUpdatedEvent`, `EmployeeCertificationUpdatedEvent` (4 types) | Same 4 types, 1:1 | **One-to-one**. |
| Machine | `MachineCreatedEvent`, `MachineUpdatedEvent` (+`MachineUpdateKind`), `MachineMaintenanceUpdatedEvent`, `MachineFailureRecordedEvent` (4 types) | Same 4 types, 1:1 | **One-to-one**. |

### 3.2 Findings

**F3-01 — Facility's event-collapsing pattern is not mirrored elsewhere. (Highest-impact finding in this section.)**
Facility uses a many-to-one mapping (4 domain events → 1 `FacilityLifecycleChangedEvent`, discriminated by `NewStatus`), matching the discriminated-union shape already used for `FacilityUpdatedEvent`/`FacilityUpdateKind`. Employee and Machine keep a flat 1:1 mapping instead. Neither approach is wrong, but having both in the same program is an inconsistency a Phase 6 implementer would have to guess about. **This needs an explicit decision in the consolidated spec** (§7.3): either (a) standardize on "collapse same-shape event variants behind one Kind-discriminated integration event," and retrofit nothing (Employee/Machine's events are not same-shape enough to warrant collapsing today), or (b) leave Facility's lifecycle collapse as a one-off exception, documented as such. Recommendation below in §7.3 is (b) — Facility's four lifecycle events genuinely share one shape (`FacilityId`, new status, optional reason); Employee/Machine's event sets do not share a common shape, so forcing a collapse there would lose information.

**F3-02 — No event carries `CorrelationId` or `CausationId` as part of its own payload.**
Every integration event (`IModuleEvent`) requires only `SourceModule` and `OccurredAtUtc`. Correlation is passed as a *separate* parameter to `PublishIsolatedAsync(event, logWriter, correlationId, ct)` and only reaches a **log line**, never the event object a subscriber actually receives. `CausationId` does not exist anywhere in this codebase as a concept. A subscriber that receives `FacilityCreatedEvent` today has no way to know what request/command caused it, and no way to correlate it against other events from the same causal chain. This is a real gap against the stated event-hardening goal ("events must include correlationId and causationId") and needs a concrete fix in Phase 6: add `string CorrelationId` to `IModuleEvent` itself (causationId can be deferred — nothing in this codebase chains commands off other commands yet, so there is no real causation to carry).

**F3-03 — No event versioning field exists anywhere.**
`IModuleEvent` has no `SchemaVersion`/`EventVersion` property, and no concrete event type defines one either. For a single-process, in-memory event bus with no persistence (see F3-05), this has not mattered yet — but it is a real gap the moment any event is persisted, queued, or consumed by a process that didn't ship in lockstep with the publisher.

**F3-04 — Cross-module consumption is effectively unimplemented today.**
The *only* real subscriber registered anywhere in the host is a single `Subscribe<FacilityCreatedEvent>()` call in `Program.cs` that writes a log line. Every other integration event — all of Employee's, all of Machine's, and three of Facility's four — has zero subscribers. This is not a defect in the Phase 4 work (publishing was the explicit, scoped goal; consuming was not), but it means "cross-module event consumption," as a working pattern, exists in exactly one place and has not been proven at scale.

**F3-05 — No event persistence/outbox exists; deterministic replay is not possible today.**
`ModuleEventGateway` is a pure in-memory pub/sub (`ConcurrentDictionary<Type, List<Handler>>`) with no storage. Every event vanishes the instant `PublishAsync` returns. "Missing events required for deterministic replay" (the spec's own phrasing) is the wrong frame — there is currently no replay mechanism *at all* to be missing events from. This is a structural gap, not an event-shape gap, and is out of scope for a contract-naming pass; flagged here so it isn't mistaken for something a naming/versioning fix could solve.

---

## 4. Repository & Persistence Contract Review

### 4.1 Inventory

| Module | Interface | Implementation | Backing store | On `ISqlGateway`? |
|---|---|---|---|---|
| Facility | `IFacilityRepository` | `FacilityRepository` | In-memory `IFacilityStore` (singleton) | N/A — no SQL |
| Employee | `IEmployeeRepository` | `EmployeeRepository` | In-memory `IEmployeeStore` (singleton) | N/A — no SQL |
| Machine | `IMachineRepository` | `MachineRepository` | In-memory `IMachineStore` (singleton) | N/A — no SQL |
| SafetyData | `ISafetyDataRepository` | `SafetyDataDapperRepository` | SQL Server, `PlantSafety.CoreDB` | **Yes** (migrated Phase 4.2) |
| AuditLogging | `IAuditLogRepository` | `AuditLogDapperRepository` | SQL Server, `PlantSafety.AuditDB` | **No** |

### 4.2 Findings

**F4-01 — `AuditLogDapperRepository` has the exact gap `SafetyDataDapperRepository` had before Phase 4.2.**
It opens its own `SqlConnection` directly from `AuditLoggingOptions.AuditDbConnectionString`, has no retry policy, no circuit breaker, and logs failures via a raw `ILogger<AuditLogDapperRepository>.LogError(...)` call instead of `ISqlGateway`'s structured `SqlDiagnosticsLogEventFactory` telemetry. This is a direct, concrete candidate for the same migration `SafetyDataDapperRepository` already received — not a hypothetical parallel, the exact same shape, same fix. `ISqlGateway` currently assumes one connection string for the whole process (`Shared:SqlGateway:ConnectionString`), and `AuditLog` lives in a **different database** (`PlantSafety.AuditDB`, not `PlantSafety.CoreDB`) — a multi-database `ISqlGateway` (or a second gateway instance bound to a second connection string, mirroring the `SqlCircuitBreaker` isolation pattern from Phase 3.6) needs to be designed before this migration can happen. This is real, scoped, Phase-6-shaped work — not done here.

**F4-02 — Facility/Employee/Machine have zero durability.**
All three repositories are backed by a process-local in-memory store. Every facility, employee, and machine created through the API is lost on process restart. This has been true since Path B/C and is unchanged by any Phase 4 work — restated here because a "persistence contract review" that didn't say so plainly would be misleading. Whether/when these three domains get real SQL persistence is a product/sequencing decision, not something this audit can resolve.

**F4-03 — No EF entities, no dynamic typing, no leaked SQL detected anywhere in scope.**
Verified across all five repositories: no `DbContext`, no `dynamic`, no string-built SQL (`SafetyDataDapperRepository`/`AuditLogDapperRepository` both call stored procedures exclusively via `CommandType.StoredProcedure`). Compliant.

---

## 5. Cross-Module Policy Alignment

### 5.1 Already Centralized (verify-and-preserve)

- **Retry/circuit-breaker policy** (`Shared.ErrorHandling`'s `ErrorRetryPolicy.Standard`, `CircuitBreakerPolicy.Standard`) is shared code, correctly instantiated as **separate instances per dependency** (`GlobalErrorHandler`'s whole-API breaker vs. `SqlCircuitBreaker`'s SQL-specific breaker, Phase 3.6) rather than one instance shared across unrelated failure domains. This is the correct pattern and should remain the rule for any future dependency-specific breaker.
- **`SeverityLevel`** is defined once (`Shared.FacilityDomain.Abstractions.Severity`) and referenced directly by `WorkflowEngine.Abstractions.Engine.WorkflowContext` rather than duplicated — correct cross-module reuse.

### 5.2 Duplicated, Should Be Centralized

**F5-01 — Required-string validation is copy-pasted six times.**
`FacilityAggregate`, `EmployeeAggregate`, and `MachineAggregate` each independently implement the identical pattern:
```csharp
if (string.IsNullOrWhiteSpace(x))
    throw new ArgumentException("{X} is required.", nameof(x));
```
(Facility: Name, ShutdownReason, DecommissionReason — 3 occurrences. Employee: Name, InjuryDescription — 2. Machine: Name, FailureDescription — 1. Six occurrences total, byte-for-byte identical shape.) A single `Guard.AgainstNullOrWhiteSpace(value, paramName, entityDescription)` helper in a shared, dependency-free location (`Shared.Deterministic` or a new minimal `Shared.Guards` project) would remove the duplication without introducing a new bounded context.

### 5.3 Not Applicable Today

**F5-02 — Authorization policy alignment: nothing to align.**
No authentication or authorization exists anywhere in the host (standing fact since Phase 3.9). "Identify policies that should be centralized" has no candidates for authorization specifically — this stays flagged, not fixed, pending a real auth-system decision.

---

## 6. Enterprise Rule Handbook Compliance

**No "Enterprise Rule Handbook" document was found anywhere in this program's repository.** Every reference to that phrase found during this audit resolves to files inside the unrelated, separately version-controlled `Intelligence/` and `PlantSafety/` repositories (§ Scope Note) — not this program. Section 6 is therefore written against the enums and rule objects that actually exist in code, without asserting compliance against an external document this audit could not locate. If such a handbook exists outside this repository, it needs to be supplied before a real compliance check can be performed.

### 6.1 Enum Inventory (audited scope only)

| Enum | Module | Members | Suffix convention |
|---|---|---|---|
| `FacilityLifecycleStatus` | Facility | PendingActivation, Active, ShutDown, Decommissioned | *Status* (lifecycle) |
| `FacilityUpdateKind` | Facility | Metadata, HazardProfile, RegulatoryProfile, ProgramProfile | `*Kind` (event discriminator) |
| `FacilityHierarchyChangeKind` | Facility | BuildingAdded, AreaAdded, ZoneAdded, Replaced | `*Kind` |
| `HazardType` | Facility | (Fire, ChemicalSpill, HotWork, ConfinedSpace, LOTOFailure, …) | `*Type` (entity classification) |
| `HazardCategory` | Facility | — | `*Category` |
| `SeverityLevel` | Facility (shared) | (…, Severe, Catastrophic) | `*Level` |
| `RegulatoryJurisdiction` | Facility | — | — |
| `EmployeeRole` | Employee | (Operator, Supervisor, Safety, Administrator, …) | — |
| `EmployeeUpdateKind` | Employee | Metadata, InjuryProfile, ExposureRecords, SafetyScore | `*Kind` |
| `MachineType` | Machine | (PressureVessel, Boiler, ConveyorSystem, …) | `*Type` |
| `MachineUpdateKind` | Machine | Metadata, SafetyScore | `*Kind` |
| `EvacuationScope` | WorkflowEngine | (Zone, Area, Building, Facility) | `*Scope` |
| `NotificationChannel` | WorkflowEngine | (Email, Sms, Page) | `*Channel` |
| `WorkflowStepStatus` | Shared.Workflow | Completed, Failed, RolledBack, Skipped | `*Status` |
| `ErrorCategory` | Shared.ErrorHandling | Validation, NotFound, Conflict, Unauthorized, Dependency, Internal | `*Category` |
| `CircuitState` | Shared.ErrorHandling | Closed, Open, HalfOpen | — |
| `LogSeverity` / `LogCategory` | Shared.Telemetry | — | `*Severity` / `*Category` |

### 6.2 Findings

**F6-01 — Suffix conventions are 100% consistent but unwritten.** Every `*Kind` enum discriminates an event/change; every `*Type` enum classifies an entity; `*Status` is reserved for lifecycle/execution state machines; `*Category`/`*Level`/`*Scope`/`*Channel` are each used exactly once per concept, never mixed. No enum needs renaming, merging, or splitting — the convention already holds. It needs to be **written down** so it keeps holding once more people/modules touch this code (§7.6).

**F6-02 — `EmployeeUpdateKind.SafetyScore` and `MachineUpdateKind.SafetyScore` are parallel, independently-defined members.** Not a defect (Employee and Machine safety scores are genuinely different domains' data), but worth noting: if a third aggregate needs the same "recompute produces its own Kind member" shape, consider whether a shared marker convention is worth introducing then — not now, only two data points exist today.

---

## 7. Phase 5 Output: Consolidated Contract Specification

*This section is the deterministic, ready-for-Phase-6 ruleset. Every rule below either codifies a pattern already followed 100% of the time in the audited code (§7.1, §7.2, §7.4, §7.6) or resolves a specific finding above (§7.3, §7.5, §7.7) with an explicit decision rather than leaving it open.*

### 7.1 Domain Model Naming Rules
1. Aggregate root: `{Entity}Aggregate` (sealed class, private constructor, static factory `Create{Entity}`, static `Rehydrate`).
2. Immutable snapshot: `{Entity}State` (sealed record).
3. Strong ID: `{Entity}Id` (sealed record or readonly struct with `.New()` and `.From(Guid)`).
4. Lifecycle enum, where an entity has one: `{Entity}LifecycleStatus`. Every member is a single capitalized word — no compound casing (fixes F1-02: rename `ShutDown` → `Shuttered` or `Suspended` in Phase 6, since `Shutdown` collides with the existing `ShutdownFacility` method name if simply re-cased).
5. Every mutating aggregate method takes `DateTime nowUtc` as its final parameter and never calls `DateTime.UtcNow`/`DateTime.Now` internally. No exceptions.
6. Required-string validation goes through the shared guard helper introduced to resolve F5-01 (`Guard.AgainstNullOrWhiteSpace`), not an inline `if`/`throw`.

### 7.2 Command/Query Naming Rules
1. State-changing operations that **replace** current state: `Update{Aspect}Async`.
2. State-changing operations that **append** an immutable historical fact: `Record{Fact}Async`. (Codifies F2-01 — do not collapse this distinction.)
3. Creation: `Create{Entity}Async`. Single-item read: `Get{Entity}Async` (nullable return). Collection read: `GetAll{Entity}sAsync`.
4. No command or query method may accept or return a SQL/ORM-specific type, connection string, or raw persistence row shape (already 100% compliant — keep it that way).

### 7.3 Event Naming/Versioning Rules
1. Domain event (Abstractions layer, pure fact, no `IModuleEvent`): `{Entity}{PastTenseVerb}Event`.
2. Integration event (Infrastructure layer, implements `IModuleEvent`): same name, different namespace — do not rename between domain and integration layers (current convention, keep it).
3. **Collapsing rule (resolves F3-01):** domain events that share an identical payload shape and differ only by a state/kind discriminator (Facility's four lifecycle events → `FacilityLifecycleChangedEvent`) **should** collapse into one integration event with a `string NewStatus`/`Kind` field. Domain events with genuinely different payload shapes (Employee's Training/Certification/Injury events; Machine's Maintenance/Failure events) **should not** be forced into one type merely to match Facility's shape. Facility's collapse is the exception that proves correct, not the rule to blindly copy.
4. **`IModuleEvent` gains a `string CorrelationId` property** (resolves F3-02). Every `PublishIsolatedAsync` call site sets it from the same `ICorrelationContext.CorrelationId` value already being passed as a separate parameter today — this is a mechanical, low-risk addition. `CausationId` is deferred: nothing in this codebase chains one command off another yet, so there is no real value to carry.
5. **`IModuleEvent` gains an `int SchemaVersion` property, defaulted to `1`** (resolves F3-03). Every existing event becomes version 1 by definition; no migration needed today, but the field exists before it's needed.
6. Cross-module consumption (F3-04) and event persistence/replay (F3-05) are **not** naming/contract concerns — they are structural additions for a future phase, explicitly out of scope for a consolidation pass.

### 7.4 Repository Interface Rules
1. Interface: `I{Entity}Repository`. Implementation: `{Entity}Repository` (in-memory) or `{Entity}DapperRepository` (SQL-backed via `ISqlGateway`).
2. Any SQL-backed repository **must** depend on `ISqlGateway`, never open its own `SqlConnection`. (Resolves F4-01's rule; the migration itself is Phase 6 work, contingent on `ISqlGateway` gaining multi-connection-string support.)
3. In-memory-only modules (Facility/Employee/Machine today) must say so plainly in their repository's header comment — no implication of durability that doesn't exist (already true; keep it true as new repositories are added).

### 7.5 Policy Object Rules
1. A policy shared by more than one otherwise-unrelated dependency (retry, circuit breaker) gets **its own instance per dependency**, never a single shared instance across unrelated failure domains — codifies the pattern already correct in `SqlCircuitBreaker` vs. the whole-API `CircuitBreaker`.
2. A validation rule identical across ≥2 aggregates must live in a shared guard helper, not be re-implemented per aggregate (resolves F5-01).
3. Authorization/permission policy has no rule yet — deferred pending a real auth-system decision (F5-02 unchanged).

### 7.6 Enum Normalization Rules
1. `*Type` — classifies what kind of *entity* something is (`HazardType`, `MachineType`).
2. `*Kind` — discriminates which *event/change* occurred (`FacilityUpdateKind`, `EmployeeUpdateKind`, `MachineUpdateKind`, `FacilityHierarchyChangeKind`).
3. `*Status` — a lifecycle or execution state machine with guarded transitions (`FacilityLifecycleStatus`, `WorkflowStepStatus`, `CircuitState`).
4. `*Category` / `*Level` / `*Scope` / `*Channel` — single-purpose classifiers, one per concept, never reused for a second unrelated concept.
5. No enum member may use compound/mixed casing that another member of the same enum doesn't also use (resolves F1-02).

### 7.7 Completion Criteria for Phase 6
Phase 6 implementation work is ready to begin against this specification once, and only once, the following product decisions (not engineering decisions — this audit cannot resolve them) are made:
- Does Employee and/or Machine need a lifecycle status (F1-01)? If yes, what are the valid states and transitions?
- Is Facility's safety score intentionally absent, or a gap (F1-03)?
- Should `AuditLog`'s SQL migration (F4-01) come before or after `ISqlGateway` gains multi-connection-string support — i.e., is a second gateway instance/keyed-DI approach (mirroring `SqlCircuitBreaker`'s isolation pattern) the right shape, or should `ISqlGateway` itself become connection-aware per call?

Everything else in this document — naming rules, the event-collapsing decision, the correlation-id/versioning additions, the guard-helper consolidation — is fully specified and requires no further product input.
