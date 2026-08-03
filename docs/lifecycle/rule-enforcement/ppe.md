# Rule Enforcement Validation — PPE

See `_shared-findings.md`. `PPEAssignment`/`PPEInventory` (rich), 3 create-only (batched). Checked
directly this phase for a `PPECategoryId`-shaped cross-module gap on `PPEAssignment` — none found.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PPEAssignment — Create | Pass | Pass (inherited) | **Pass — checked directly this phase**: no `PPECategoryId`/other cross-aggregate FK field found on this aggregate; nothing to fail a check against | Pass | Fail (F1, pattern) | Fail (G2) — `PPEAssignmentDapperRepository` confirmed hardcoded site | High | Audit-logged (F8) |
| PPEAssignment — Assign | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PPEAssignment — RecordFitTest | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PPEAssignment — FlagForReplacement | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PPEAssignment — Retire | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PPEAssignment — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| PPEInventory — Create/IssueUnits/ReceiveStock/FlagForReplacement/Retire | **Pass** — 6 guard sites, best-guarded of the PPE group (inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `PPEInventoryDapperRepository` confirmed hardcoded site | High | |
| PPEInventory — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| PPECategory — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `PPECategoryDapperRepository` confirmed hardcoded site | Medium | Missing entirely beyond Create; dormant `_Update` unused |
| PPETrainingFit — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `PPETrainingFitDapperRepository` confirmed hardcoded site | Medium | Same shape |
| PPEZoneMapping — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `PPEZoneMappingDapperRepository` confirmed hardcoded site | Medium | Same shape |
