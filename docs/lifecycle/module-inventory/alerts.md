# Module Inventory — Alerts

**Three parallel Alert shapes confirmed to exist**, expanding the prior discovery's "3 parallel Alert
shapes" shape-audit finding with per-transition detail for the two that have real lifecycle methods.

1. `PlantSafety.Domain.Alerts.Alert.Aggregates.Alert` — the shape the ingestion discovery treated as
   "live" (wired to `Alerts` controllers).
2. `PlantSafety.Domain.Operational.Alerts.Aggregates.Alert` — **richer than the "live" one**
   (9 transitions vs 8), with its own dedicated guard-clause helper class,
   `Operational.Alerts.Aggregates.AlertLifecycleGuard`, encoding `EnsureCanAcknowledge`/
   `EnsureCanArchive`/`EnsureCanClose`/`EnsureCanEscalate`/`EnsureCanResolve`/`EnsureCanStartWork` — a
   structurally distinct (arguably better factored) guard pattern than any other PlantSafety aggregate
   uses. Which of the two is actually wired to a live controller route was **not re-confirmed this
   phase** — flagged as the top open question for Phase 2.
3. A third Alert-shaped class was noted by the prior discovery's shape-audit but not named in the
   lifecycle-coverage doc; not re-located this phase within the time budget — flagged for Phase 2.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Alerts.Alert (shape #1) | Trigger (Create) | Yes | Yes — `usp_Alert_Trigger` (dedicated transition-named proc — the only one found in all 541) | **Live** (prior discovery's controller trace) | Directly verified (inherited) | Zero input validation — `TriggerAlertCommand` has no FluentValidation validator; oversized strings silently truncated by `AddString`, not rejected |
| Alerts.Alert (shape #1) | Acknowledge (Triggered→Acknowledged) | Yes | Yes — `usp_Alert_StatusTransitions` | Live | Directly verified (inherited) | |
| Alerts.Alert (shape #1) | Dispatch (Acknowledged→Dispatched) | Yes | Yes — `usp_Alert_StatusTransitions` | Live | Directly verified (inherited) | |
| Alerts.Alert (shape #1) | Escalate | Yes | Yes — `usp_Alert_StatusTransitions` | Live | Directly verified (inherited) | |
| Alerts.Alert (shape #1) | Resolve | Yes | Yes — `usp_Alert_StatusTransitions` | Live | Directly verified (inherited) | |
| Alerts.Alert (shape #1) | Close | Yes | Yes — `usp_Alert_StatusTransitions` | Live | Directly verified (inherited) | `CK_Alert_Status` CHECK constraint backs this — best-guarded SQL column in the module |
| Alerts.Alert (shape #1) | RecordDelivery | Yes | Yes — `usp_Alert_RecordChildEntities` | Live | Directly verified (inherited) | No status guard found in this method |
| Alerts.Alert (shape #1) | RecordResponseAction | Yes | Yes — `usp_Alert_RecordChildEntities` | Live | Directly verified (inherited) | No status guard, no confirmed `AddDomainEvent` call |
| Alerts.Alert (shape #1) | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| Operational.Alerts.Alert (shape #2) | Create | Yes | Yes — generic PlantSafety CRUD proc pattern presumed (feature area not individually SQL-grepped this phase; class name collides with shape #1's SQL prefix, `usp_Alert_*`, making file-name-only cross-reference ambiguous between the two shapes) | **Unconfirmed — legacy or live not determined this phase** | Pattern-inferred (structural scan only) | **[NEW]** Flag: this shape's SQL persistence cannot be cleanly distinguished from shape #1's via file-name grep alone because both classes are named `Alert` and likely share the `usp_Alert_*` prefix pool — a genuine ambiguity, not resolved this phase |
| Operational.Alerts.Alert (shape #2) | Acknowledge, AddMetadata, Archive, AssignTo, Close, Escalate, Resolve, SetSlaExpiration, StartProgress | Yes (all 9, each backed by an `EnsureCan*` guard in `AlertLifecycleGuard`) | Same ambiguity as above | Unconfirmed | Directly verified (method/guard presence, via this phase's structural scan); guard semantics not individually read | **9 transitions is the richest single Alert-concept lifecycle found in the solution** — richer than the shape presumed "live." If this shape is actually dead code, it is the single largest unused well-guarded lifecycle found to date; if it is actually the live one and shape #1 is legacy, the prior discovery's "live" designation needs correction. **Top Phase 2 priority for this feature area.** |
| Operational.Alerts.Alert (shape #2) | Cancel / Delete | **No** | N/A | N/A | Directly verified | |

**Bottom line for this feature area:** the prior discovery's controller-trace method (reading
`AlertsV1Controller.cs` and confirming route names) is the only way to resolve which Alert shape is
actually live — this phase's file-name-only SQL cross-reference cannot disambiguate two same-named
classes sharing a SQL prefix. Recommended as the single highest-value follow-up read for Phase 2 in
this feature area.
