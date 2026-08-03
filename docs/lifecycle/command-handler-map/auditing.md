# Command-Handler Map — Auditing

See `_shared-findings.md` for F7 (Outbox universal), F8 (**neither `AuditEngagement` nor
`AuditFinding` is on the 23-aggregate SQL-audit list — directly confirmed this phase** by opening
both `usp_AuditEngagement_Create/Update.sql` and `usp_AuditFinding_Create/Update.sql`, neither
contains the AuditDB insert block), F9 (event delivery, [EVT-B]), F10, and
`domain-mapping/_shared-findings.md` F6 (`CloseAuditEngagementCommandHandler` accepts unchecked
`ComplianceScore`/`ComplianceRating`, no query against Compliance module state).
**`AuditEngagement` fully directly verified this phase** (full read of `AuditEngagement.cs`).

| Aggregate | Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| AuditEngagement | Create | `CreateAuditEngagementCommand` (pattern-inferred) | matching handler | `AddAsync` | `AuditEngagementCreatedEvent` | Y | **N** (F8, directly verified this phase) | Directly verified (domain method) | |
| AuditEngagement | Schedule | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | `AuditEngagementScheduledEvent` | Y | **N** | Directly verified (domain method) | Guard: must be `Created`; `ScheduledAtUtc` non-empty |
| AuditEngagement | Start | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | `AuditEngagementStartedEvent` | Y | **N** | Directly verified (domain method) | Guard: must be `Scheduled` or `Created` |
| AuditEngagement | RecordFinding | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | `AuditEngagementFindingRecordedEvent` | Y | **N** | Directly verified (domain method) | Guard: must be `Active`; increments `FindingsCount` |
| AuditEngagement | Close | `CloseAuditEngagementCommand` (confirmed by name via F6) | `CloseAuditEngagementCommandHandler` (confirmed, F6) | `GetByIdAsync` → `UpdateAsync` | `AuditEngagementClosedEvent` | **Y** | **N** | Directly verified (domain method + F6 handler read) | **F6 (reconfirmed):** `ComplianceScore`/`ComplianceRating` are raw caller-supplied input, no cross-module query against Compliance state, no check for open `AuditFinding`s. Range guard (`0-100`) exists in the domain method itself, but that is the only validation. This is a real, reachable, event-raising transition with **zero SQL audit trail and an accepted-on-faith compliance score** — see report's "most concerning silent transition" discussion. |
| AuditEngagement | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (domain method absence, this phase) | |
| AuditFinding | Create | Command name pattern-inferred | matching handler | `AddAsync` | `AuditFindingCreatedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred (inherited counts) | |
| AuditFinding | Remediate | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| AuditFinding | TriggerCapa | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | Name suggests a cross-module touch into AccidentManagement's CorrectiveAction/CAPA concept — not confirmed this phase whether it makes a real repository call there or (like F6/F11) just stores a reference; flagged as a follow-up candidate for a future phase |
| AuditFinding | Verify | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| AuditFinding | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| Auditing (Compliance duplicate marker) | Create only | Presumed exists | Unconfirmed | Read-only SQL surface (`usp_Auditing_GetLogs` only, inherited) | Unconfirmed | N/A | **N** | Directly verified (SQL, inherited) | Distinct controller tree from the live pair (inherited) |

**Event delivery reality (F9 [EVT-B]):** `AuditingCreatedConsumer` (`publishing-map.md` line 67/
`consumption-map.md` line 30) is bound to `AuditingCreatedEvent` — the **Compliance duplicate
marker's** event — not `AuditEngagementCreatedEvent`/`AuditFindingCreatedEvent` from the live pair.
The live pair's events (Created and all non-Create types across both aggregates) have **zero
consumer class of any kind.**

**Module-level bottom line:** this is the solution's clearest case of a rich, well-guarded, genuinely
live lifecycle (`AuditEngagement`/`AuditFinding`, both directly re-verified this phase) that gets
**zero** SQL-level audit logging and **zero** event-consumer coverage — every one of its 9 transitions
combined produces only an Outbox row nobody reads. `Close` additionally accepts a compliance score on
faith (F6). This module's audit/trace posture is worse than modules with far shallower lifecycles
(e.g. Accident, Risk, SafetyPolicy all get the SQL AuditLog row at least).
