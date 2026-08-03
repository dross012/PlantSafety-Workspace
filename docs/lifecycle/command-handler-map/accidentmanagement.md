# Command-Handler Map — AccidentManagement

See `_shared-findings.md` for F7 (Outbox universal), F8 (SQL AuditLog covers only 23 aggregates —
`Accident` is one of them), F9 (event delivery, [EVT-C] for this module), F10 (file-layout pattern).
1 aggregate: `Accident`. **Directly verified this phase** by full read of
`Commands/AccidentInvestigationCommands.cs`, `Commands/AccidentInvestigationCommandHandlers.cs`, and
`PlantSafety.Domain\AccidentManagement\Aggregates\Accident.cs`.

| Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create (Report) | `CreateAccidentCommand` | `CreateAccidentCommandHandler` | `IAccidentRepository.AddAsync` | `AccidentCreatedEvent` | Y | **Y** (SQL, `usp_Accident_Insert`, F8) | Directly verified | |
| AssignInvestigator | `AssignAccidentInvestigatorCommand` | `AssignAccidentInvestigatorCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentInvestigatorAssignedEvent` | Y | **Y** (`usp_Accident_Update`) | Directly verified | |
| RecordRootCause | `RecordAccidentRootCauseCommand` | `RecordAccidentRootCauseCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentRootCauseRecordedEvent` | Y | **Y** | Directly verified | |
| CreateCorrectiveAction | `CreateAccidentCorrectiveActionCommand` | `CreateAccidentCorrectiveActionCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentCorrectiveActionCreatedEvent` | Y | **Y** | Directly verified | |
| CompleteCorrectiveAction | `CompleteAccidentCorrectiveActionCommand` | `CompleteAccidentCorrectiveActionCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentCorrectiveActionCompletedEvent` | Y | **Y** | Directly verified | Does not re-check parent `Accident.Status` (inherited Phase 1/2 finding) — the audit row would still be written even though the transition itself is under-guarded |
| FileRegulatoryReport | `FileAccidentRegulatoryReportCommand` | `FileAccidentRegulatoryReportCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentRegulatoryFilingCompletedEvent` | Y | **Y** | Directly verified | |
| CloseInvestigation | `CloseAccidentInvestigationCommand` | `CloseAccidentInvestigationCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AccidentInvestigationClosedEvent` | Y | **Y** | Directly verified | |
| Archive (Closed→Archived) | **None — no command/handler class exists anywhere in `PlantSafety.Application.AccidentManagement`** | N/A | N/A — unreachable from any command | N/A — domain method exists (`Accident.Archive`) but is never invoked; if it were, it would not call `AddDomainEvent` (F3, inherited silent-archive) | N/A | N/A (never executes) | **Directly verified (new this phase)** | **The most concrete "fully dead" transition found in this module**: the domain guard exists and is even correctly factored, but there is no Application-layer entry point at all — grepped the entire `AccidentManagement` Application folder for "archive" (case-insensitive) and found only unrelated DTO property names. Doubly dead: unreachable AND (if it were somehow reached) silent. See report's "most concerning silent transition" discussion. |
| Delete / Cancel | Missing entirely (domain) | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-C], cross-referenced from `events/publishing-map.md` line 84-90 and
`events/consumption-map.md` line 47):** all 7 events above are constructed and reach the Outbox table,
but **zero consumer class of any kind exists** for any of them (not even an unregistered stub) — grep
of `PlantSafety.Infrastructure` for `Accident*Consumer` returns zero results. Every one of these 7
events is a dead end downstream of Outbox regardless of the Audit-Entry column.

**Audit summary for this module:** 6 of 7 reachable transitions (all but the unreachable `Archive`)
write a SQL `AuditLog` row — the best audit coverage of any module in the solution, but still subject
to F8's caveats (best-effort/swallowed-on-failure, `ChangedBy=SYSTEM_USER` not the real actor,
`Module=usp_Accident_Update` cannot distinguish *which* of the 6 `_Update`-backed transitions fired).
