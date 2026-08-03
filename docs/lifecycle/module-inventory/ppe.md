# Module Inventory — PPE

5 aggregates: `PPEAssignment`, `PPEInventory` (rich); `PPECategory`, `PPETrainingFit`, `PPEZoneMapping`
(create-only). Note: `EmployeeSafety.Ppe` (a 6th, differently-named PPE-concept aggregate) is covered in
`employeesafety.md` — possible conceptual overlap with `PPEAssignment`, not confirmed as a duplicate this
phase.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PPEAssignment | Create / Assign / RecordFitTest / FlagForReplacement / Retire | Yes (all 5, 3 guard + 6 event sites) | Yes — `usp_PPEAssignment_Insert`/`_Update` | Live | Directly verified (counts, inherited) | |
| PPEAssignment | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| PPEInventory | Create / IssueUnits / ReceiveStock / FlagForReplacement / Retire | Yes (all 5, 6 guard + 6 event sites — best-guarded of the PPE group) | Yes — `usp_PPEInventory_Insert`/`_Update` | Live | Directly verified (counts, inherited) | |
| PPEInventory | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| PPECategory | Create only | Yes | Yes — `usp_PPECategory_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| PPETrainingFit | Create only | Yes | Yes — `usp_PPETrainingFit_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| PPEZoneMapping | Create only | Yes | Yes — `usp_PPEZoneMapping_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| EmployeeSafety.Ppe | Create / FlagForReplacement / RecordFitTest / RecordInspection / Retire | Yes (all 5) | Yes — see `employeesafety.md` | Live (unconfirmed vs. `PPEAssignment`) | Directly verified (this phase) | Method names (`RecordFitTest`, `FlagForReplacement`, `Retire`) closely mirror `PPEAssignment` — flagged as a possible undocumented duplicate/overlap, not confirmed this phase |
