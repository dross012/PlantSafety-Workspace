# Command-Handler Map — Alerts

See `_shared-findings.md` for F7 (Outbox universal), F8 (SQL AuditLog — **neither** Alert shape is
on the 23-aggregate covered list), F9 (event delivery — [EVT-A] for shape #1's `Trigger`/Create,
[EVT-B] for the rest), F10 (naming convention), and `domain-mapping/_shared-findings.md` F4 (Alert
duplicate cluster — shape #1 live, shape #2 unreachable from any HTTP route).
Command/handler class names below are **pattern-inferred** from the F10 convention
(`<Verb>Alert...Command[Handler]`); not individually opened this phase for either shape.

## Shape #1 — `Alerts.Alert` (confirmed live, F4)

| Transition | Command (pattern-inferred) | Handler (pattern-inferred) | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Trigger (Create) | `TriggerAlertCommand` | `TriggerAlertCommandHandler` | `AddAsync` (pattern-inferred, F1 shape) | `AlertCreatedEvent` | Y | **N** (Alert not on F8's 23-aggregate list) | Pattern-inferred (command exists — confirmed by name in inherited Phase 1 table; handler body not re-opened) | Zero input validation (inherited); oversized strings silently truncated by `StoredProcedureBuilder.AddString` — a real correctness bug independent of audit/outbox |
| Acknowledge | `AcknowledgeAlertCommand` | `AcknowledgeAlertCommandHandler` | `GetByIdAsync` → `UpdateAsync` (F1 pattern) | `AlertAcknowledgedEvent` (pattern-inferred name) | Y | **N** | Pattern-inferred | |
| Dispatch | `DispatchAlertCommand` | `DispatchAlertCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AlertDispatchedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| Escalate | `EscalateAlertCommand` | `EscalateAlertCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AlertEscalatedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| Resolve | `ResolveAlertCommand` | `ResolveAlertCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AlertResolvedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| Close | `CloseAlertCommand` | `CloseAlertCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `AlertClosedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | `CK_Alert_Status` CHECK constraint backs this at the SQL layer (inherited) — the one place a DB-level guard exists independent of the C# guard |
| RecordDelivery | `RecordAlertDeliveryCommand` | `RecordAlertDeliveryCommandHandler` | `GetByIdAsync` → `UpdateAsync` | Unconfirmed — inherited finding notes no status guard found in this method | Unconfirmed | **N** | Pattern-inferred | No confirmed `AddDomainEvent` call (inherited) — if true, Outbox = N too; flagged, not independently re-verified this phase |
| RecordResponseAction | `RecordAlertResponseActionCommand` | `RecordAlertResponseActionCommandHandler` | `GetByIdAsync` → `UpdateAsync` | Unconfirmed — inherited finding: "no confirmed `AddDomainEvent` call" | Unconfirmed | **N** | Pattern-inferred | Same caveat as RecordDelivery |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9):** `AlertCreatedEvent` is [EVT-A] — `AlertsCreatedConsumer` exists but
is never registered (`AddConsumer<AlertsCreatedConsumer>()` missing) and would receive the wrong CLR
type from `OutboxProcessor` even if it were. The 6 non-Create events are [EVT-B] — no consumer class
of any kind. Net: **zero live delivery for any Alert (shape #1) transition.**

## Shape #2 — `Operational.Alerts.Alert` (F4: guarded, richest lifecycle in the solution, but 0 live HTTP routes)

| Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create | Unconfirmed whether a command/handler exists at all — F4 confirms no controller route reaches this shape | N/A | N/A | N/A | N/A (if a command exists but nothing routes to it, it never executes) | N/A | Directly verified (F4, inherited) | |
| Acknowledge, AddMetadata, Archive, AssignTo, Close, Escalate, Resolve, SetSlaExpiration, StartProgress (9 total) | Unconfirmed | N/A | N/A | Per shared-findings F3: `Archive` specifically confirmed to call `AddDomainEvent` (`AlertArchivedEvent`) if ever reached; the other 8 not individually re-checked | N/A (no route reaches any of them) | N/A | Directly verified (F4: 0 HTTP routes for this shape, inherited) | **This entire 9-transition lifecycle is 100% unreachable from any HTTP route** (`AlertsV1Controller.cs`/`AlertsV2Controller.cs` under `Operational\Alerts` confirmed GET-only, one stub). Every column above is moot in production regardless of what the code would do if called — the single largest "fully guarded, fully dead" surface in the solution. |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Bottom line:** shape #1 delivers zero observable downstream effect from any transition (F9); shape
#2 delivers zero effect because it cannot even be invoked (F4). Between the two, no Alert-concept
transition in this solution produces a real cross-system reaction today.
