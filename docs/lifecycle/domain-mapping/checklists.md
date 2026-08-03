# Domain Mapping — Checklists

See `_shared-findings.md` §F1 (tenancy).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| ChecklistTemplate | Create | Starts `Draft` | Y (trivial) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| ChecklistTemplate | AddItem | Must be `Draft` (pattern-inferred) | Pattern-inferred Y (guard directly verified, event pattern-inferred, Phase 1) | Item text required (pattern-inferred) | None | Not enforced (F1) | Directly verified (guard, inherited) | |
| ChecklistTemplate | Publish | Rejects publishing an empty template (zero items) — Phase 1 confirmed real rule | **Y** — confirmed | — | None | Not enforced (F1) | Directly verified (inherited) | Positive control |
| ChecklistTemplate | Retire | Must be `Published` (pattern-inferred) | Pattern-inferred Y (guard directly verified) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| ChecklistTemplate | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| ChecklistCompletion | Create | Linked to a `ChecklistTemplate` (pattern-inferred) | Pattern-inferred Y | TemplateId required | Should verify the referenced `ChecklistTemplate` is `Published` (not `Draft`/`Retired`) — no confirmed cross-aggregate check | Not enforced (F1) | Directly verified (inherited) | Candidate cross-module/cross-aggregate gap — a completion could plausibly be started against a Draft or Retired template |
| ChecklistCompletion | Start | Must be pre-start state (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| ChecklistCompletion | RecordItemResult | Must be `InProgress` (pattern-inferred) | Pattern-inferred Y | Item result value required | None | Not enforced (F1) | Directly verified (inherited) | |
| ChecklistCompletion | FlagFailedItem | Guard not independently confirmed (Phase 1 finding) | **Unconfirmed** | — | None | Not enforced (F1) | Pattern-inferred | Recommend direct read in Phase 3 |
| ChecklistCompletion | Complete | Should require all items recorded (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| ChecklistCompletion | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| Checklists (Operational duplicate) | Create only | Factory-only | N/A | None | None | Not enforced (F1) | Directly verified (SQL check, Phase 1) | Confirmed dead-ish: read-only SQL, same shape as `Operational.RiskManagement` |

**Summary:** The `ChecklistCompletion.Create`→`ChecklistTemplate` status cross-check is a plausible,
unconfirmed cross-module/cross-aggregate gap flagged this phase for the first time — worth a direct
handler read in Phase 3 to confirm whether the referenced template's status is ever checked.
