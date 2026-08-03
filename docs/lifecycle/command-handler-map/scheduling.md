# Command-Handler Map — Scheduling

See `_shared-findings.md` for F7 (Outbox universal), F8 (**all 5 aggregates in this file are on the
23-aggregate SQL-audit list**: `PMScheduling`, `WorkOrderPriority`, `EmergencyDrillPlanning`,
`HazardIsolationSequencing`, `ShiftFatigue` all confirmed present), F9 (event delivery, [EVT-A] for
`SchedulesCreatedEvent`'s naming collision, [EVT-B] otherwise), F10, and
`domain-mapping/_shared-findings.md` (the inherited "one unreachable Cancel" finding).
Command/handler names pattern-inferred; not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| PMScheduling | Create (Draft) | `CreatePMSchedulingCommand` | matching handler | `AddAsync` | `PMSchedulingCreatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| PMScheduling | SchedulePreventiveMaintenance | `SchedulePreventiveMaintenanceCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PMScheduling | RecordCompletion | `RecordPMSchedulingCompletionCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PMScheduling | Escalate | `EscalatePMSchedulingCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PMScheduling | Cancel | `CancelPMSchedulingCommand` — **existence itself is the open question**: the domain method (`PMScheduling.Cancel`) is fully guarded and confirmed to correctly raise `PMSchedulingCancelledEvent`, but no route in `SchedulingV1Controller.cs` reaches it | If a command/handler pair exists, it is unreachable from any HTTP route | `GetByIdAsync` → `UpdateAsync` (would run correctly if invoked) | `PMSchedulingCancelledEvent` — would be raised and outboxed if ever reached | Y (if reached) / effectively N (never reached in production) | **Y (SQL, if reached)** | Directly verified (inherited, F1/F2's headline finding) | This is the single best-instrumented transition in the entire solution on paper (guarded, eventing, audited) and simultaneously **100% dead in production** — same class of waste as `Operational.Alerts.Alert`'s 9 transitions (F4), just one row instead of nine |
| WorkOrderPriority | Create / Escalate / Resolve / StartWork / Close | matching commands | matching handlers | `AddAsync` (Create) / `GetByIdAsync`→`UpdateAsync` (rest) | pattern-inferred names | Y | **Y** | Pattern-inferred | |
| WorkOrderPriority | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| EmergencyDrillPlanning | Create only | `CreateEmergencyDrillPlanningCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | `_Update` proc exists as dormant capacity — no domain method exercises its audit/outbox path |
| HazardIsolationSequencing | Create only | `CreateHazardIsolationSequencingCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Same dormant-`_Update` note |
| ShiftFatigue | Create only | `CreateShiftFatigueCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Same dormant-`_Update` note |

**Event delivery reality (F9):** `SchedulesCreatedConsumer` (`publishing-map.md` line 79/
`consumption-map.md` line 42) is bound to `SchedulesCreatedEvent`, which by naming does not exactly
match any of these 5 aggregates' own `Created` event names — most likely another
Operational-namespace marker aggregate rather than any of these 5 (not individually disambiguated
this phase; flagged for a future pass). Regardless of that ambiguity, **no consumer of any kind
exists for `PMScheduling`'s 5 transitions, `WorkOrderPriority`'s 5, or the 3 Create-only aggregates'
single event each** — confirmed no `PMScheduling*Consumer`/`WorkOrderPriority*Consumer`/etc. Net:
zero live delivery for every transition in this module, including the best-instrumented-but-unreachable
`Cancel`.

**Audit summary for this module:** this is the **only module in the solution where every single
aggregate is on the F8 SQL-audit-covered list** — all 5 aggregates, all transitions (including the
unreachable `Cancel`, if it were ever reached) would write a best-effort/anonymous `AuditLog` row.
