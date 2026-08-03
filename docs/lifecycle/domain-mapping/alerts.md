# Domain Mapping — Alerts

See `_shared-findings.md` §F4 for the full resolution of the Alert-duplicate open question (direct
controller reads this phase). §F1 (tenancy), §F3 (silent archive — N/A for shape #1, no Archive
method) also apply.

## Shape #1 — `Alerts.Alert` (CONFIRMED LIVE — `/api/v1/alerts/...`, all 8 mutations wired)

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Alerts.Alert | Trigger (Create) | None confirmed beyond required fields | Partial | **Confirmed gap (inherited)**: `TriggerAlertCommand` has no FluentValidation validator; oversized strings silently truncated by `StoredProcedureBuilder.AddString`, not rejected | None | Set from envelope `TenantId`, not re-verified (F1) | Directly verified (inherited + this phase's controller read) | Zero input validation is a real, confirmed gap, not inferred |
| Alerts.Alert | Acknowledge | Must be `Triggered` or `Escalated` | Y (pattern-inferred from status enum + guard site count) | None confirmed | None | Not enforced (F1) | Directly verified (route); pattern-inferred (guard body) | |
| Alerts.Alert | Dispatch | Must be `Acknowledged` | Y (pattern-inferred) | `DispatchedTo` required (route binds it) | None | Not enforced (F1) | Directly verified (route) | |
| Alerts.Alert | Escalate | None found to prevent re-escalation | Unconfirmed | `EscalationReason` required when not auto-escalated | None | Not enforced (F1) | Directly verified (route) | |
| Alerts.Alert | Resolve | Should require Dispatch/Acknowledge first (pattern-inferred) | Pattern-inferred Y | `ResponseActionsTaken` required | None | Not enforced (F1) | Directly verified (route) | |
| Alerts.Alert | Close | Must be `Resolved` (pattern-inferred); backed by `CK_Alert_Status` CHECK constraint at the SQL layer — best-guarded SQL column in the module (Phase 1 finding) | Y (SQL-level backstop confirmed even if C# guard were bypassed) | `CloseOutVerification` required | None | Not enforced (F1) | Directly verified (inherited + route) | Positive control: DB-level CHECK constraint is a real defense-in-depth measure, rare in this codebase |
| Alerts.Alert | RecordDelivery | **No status guard found** (Phase 1 finding, inherited) | **N** — confirmed gap | Channel/Succeeded/FailureReason bound from request | None | Not enforced (F1) | Directly verified (inherited) | Can record a delivery attempt on an Alert in any status, including Closed |
| Alerts.Alert | RecordResponseAction | **No status guard, no confirmed `AddDomainEvent`** (Phase 1 finding) | **N** — confirmed gap on both counts | Action description/notes bound from request | None | Not enforced (F1) | Directly verified (inherited) | Same category as `CompleteCorrectiveAction` in AccidentManagement — silently accepted regardless of state |
| Alerts.Alert | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

## Shape #2 — `Operational.Alerts.Alert` (CONFIRMED THIS PHASE: no live mutation route exists — F4)

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Operational.Alerts.Alert | Create | Unconfirmed SQL destination (ambiguous prefix collision, Phase 1) | Unconfirmed | Unconfirmed | None | Not enforced (F1) | Pattern-inferred | No route found this phase either |
| Operational.Alerts.Alert | Acknowledge/AddMetadata/AssignTo/Close/Escalate/Resolve/SetSlaExpiration/StartProgress | Each backed by a dedicated `AlertLifecycleGuard.EnsureCan*` method — the best-factored guard pattern in the solution | **Guards exist and are real** (own helper class), but **provably unreachable** — confirmed this phase: no controller anywhere exposes a POST/PUT for this shape | Not individually read this phase | None | Not enforced (F1) | Directly verified (dead-route finding, this phase); guard bodies not individually re-opened | The richest guarded lifecycle in the solution, 100% dead from the API's perspective — see F4 |
| Operational.Alerts.Alert | Archive | Guard via `EnsureCanArchive`; **confirmed this phase to call `AddDomainEvent`** (`AlertArchivedEvent`) — does NOT follow the silent-archive pattern (F3) | Y for guard+event; N for reachability (dead route) | — | None | Not enforced (F1) | Directly verified (method body read this phase) | Resolves Phase 1's 3rd open Archive question for this aggregate |
| Operational.Alerts.Alert | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary for this file:** Shape #1 has 2 confirmed "no guard at all" transitions (RecordDelivery,
RecordResponseAction) plus 1 confirmed zero-validation Create. Shape #2's entire 9-transition
lifecycle — despite being correctly guarded and, for Archive, correctly event-emitting — is dead
code at the HTTP boundary, the 2nd-clearest "looks safe, isn't reachable" finding after Intelligence.
The 3rd Alert shape flagged in Phase 1 was not located this phase either.
