# Rule Enforcement Validation — Alerts

See `_shared-findings.md` (G1-G6) and `domain-mapping/_shared-findings.md` F4 (Alert duplicate,
resolved: shape #1 live, shape #2 dead-from-API). 2 real shapes + 1 unresolved 3rd.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Alerts.Alert #1 — Trigger (Create) | **Fail** — no FluentValidation validator found for `TriggerAlertCommand`; oversized strings silently truncated, not rejected, by `StoredProcedureBuilder.AddString` (inherited) | **Fail (G3)** — no validator class exists for this command at all | Pass | Pass (no predecessor) | Fail (F1/G1) | Fail (G2) — `AlertDapperRepository` is one of the 58 hardcoded-Enqueue sites | High | Constraints gap is structural absence, not a wiring defect |
| Alerts.Alert #1 — Acknowledge | Pass | Unconfirmed — not individually checked for a dedicated validator this phase | Pass | Pass | Fail (F1) | Fail (G2) | Medium | |
| Alerts.Alert #1 — Dispatch | Pass | Unconfirmed | Pass | Pass | Fail (F1) | Fail (G2) | Medium | |
| Alerts.Alert #1 — Escalate | Pass | Unconfirmed | Pass | Pass | Fail (F1) | Fail (G2) | Medium | |
| Alerts.Alert #1 — Resolve | Pass | Unconfirmed | Pass | Pass | Fail (F1) | Fail (G2) | Medium | |
| Alerts.Alert #1 — Close | Pass — `CK_Alert_Status` SQL CHECK constraint backs this too (best-guarded SQL column in the module, inherited) | Unconfirmed | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Alerts.Alert #1 — RecordDelivery | **Fail** — no status guard found in this method (inherited) | Unconfirmed | Pass | **Fail** — can be recorded regardless of Alert status, since no guard exists to reject it in an invalid state | Fail (F1) | Fail (G2) | Medium | |
| Alerts.Alert #1 — RecordResponseAction | **Fail** — no status guard, no confirmed `AddDomainEvent` (inherited) | Unconfirmed | Pass | **Fail** — same reachability gap as RecordDelivery | Fail (F1) | N/A — no event raised, nothing to Enqueue | Medium | |
| Operational.Alerts.Alert #2 — all 9 transitions (Acknowledge/AddMetadata/Archive/AssignTo/Close/Escalate/Resolve/SetSlaExpiration/StartProgress) | Pass — each backed by a dedicated `AlertLifecycleGuard.EnsureCan*` (richest guard pattern in the solution, inherited) | Unconfirmed | Pass (guard-level; no other-module reference found) | Pass — guard-level enforcement is real | Fail (F1, pattern-inferred — same generated repository shape) | Unconfirmed — repository for this shape not located this phase | Medium | **Moot in practice**: 100% dead code from the API's perspective (F4) — every criterion above describes a lifecycle nothing can ever reach over HTTP |
| 3rd Alert shape | Not re-located this phase | — | — | — | — | — | Low | Still open per F4 |
