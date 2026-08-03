# Domain Mapping — PPE

See `_shared-findings.md` §F1 (tenancy).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PPEAssignment | Create/Assign/RecordFitTest/FlagForReplacement/Retire | 3 guard + 6 event sites | Pattern-inferred Y | Fit-test score plausible | Method names closely mirror `EmployeeSafety.Ppe` — possible undocumented duplicate, not confirmed (Phase 1 finding, not resolved this phase either — no shared identity/FK found) | Not enforced (F1) | Directly verified (counts, inherited) | |
| PPEAssignment | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PPEInventory | Create/IssueUnits/ReceiveStock/FlagForReplacement/Retire | Best-guarded of the PPE group: 6 guard + 6 event sites | Pattern-inferred Y | Stock quantity non-negative plausible | `IssueUnits` plausibly should check `PPEAssignment` doesn't already have an active unit issued — no confirmed cross-aggregate link | Not enforced (F1) | Directly verified (counts, inherited) | |
| PPEInventory | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PPECategory | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | Dormant Update SQL proc |
| PPETrainingFit | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | Same |
| PPEZoneMapping | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | Same |
| EmployeeSafety.Ppe | Create/FlagForReplacement/RecordFitTest/RecordInspection/Retire | See `employeesafety.md` — same duplicate-question as above | Pattern-inferred Y | — | Not confirmed as overlapping with `PPEAssignment` this phase | Not enforced (F1) | Directly verified (this phase, method presence) | Still open — recommend Phase 3 read of both aggregates' repository schemas to check for a shared table/FK |

**Summary:** The `PPEAssignment` / `EmployeeSafety.Ppe` overlap remains unresolved — flagged in
Phase 1, not confirmed or refuted by direct evidence this phase (would require reading both
aggregates' SQL table definitions to check for a shared physical PPE unit identity).
