# SQL Shape Audit — PPE

Phase 6 of 10. Covers `PPEAssignment`, `PPEInventory` (rich); `PPECategory`, `PPETrainingFit`,
`PPEZoneMapping` (create-only). `EmployeeSafety.Ppe` (possible duplicate) covered in `employeesafety.md`.

## Worst issues in this module

1. **`FlagPPEAssignmentForReplacementCommand.Reason` — confirmed truncation risk, an 10th solution-wide
   instance, same 600-char-gap shape as PPE/Observability's other `Reason`-family fields.**
   `FlagPPEAssignmentForReplacementCommandValidator.cs:22`: `MaximumLength(1000)`, mapped to the domain/
   column name `ReplacementReason`. `PPEAssignmentDapperRepository.cs:89,125`:
   `AddString("ReplacementReason", aggregate.ReplacementReason, 400)`. Directly verified.
2. **`FlagPPEInventoryForReplacementCommand.Reason` is validated (`MaximumLength(1000)`) but never
   persisted to `PPEInventory` at all** — `PPEInventoryDapperRepository.cs` read in full for this field:
   only `Status` and `PpeItemCode` are written on both Insert and Update; **no `ReplacementReason`/
   `Reason`-named column or `AddString` call exists anywhere in this repository.** A 3rd instance
   (alongside `SafetyPolicies.Reject`/`Retire` and `Scheduling.WorkOrderPriority.Escalate`) of a fully
   validated business reason being captured and then discarded before reaching durable storage — the
   *sibling* aggregate in the same module (`PPEAssignment`, issue #1) gets this field persisted (albeit
   truncated), making the inconsistency within one module particularly hard to defend as intentional.
   Directly verified.
3. **`RecordPPEFitTestCommand.Notes` (`MaximumLength(1000)`)** — not confirmed this phase whether it is
   persisted or at what size; flagged as an open follow-up given the module's 2-for-2 record on this
   field family so far.
4. **No CHECK constraint found on either rich aggregate's `Status` column.**
5. **Possible undocumented duplicate with `EmployeeSafety.Ppe`** (inherited, see `employeesafety.md`
   worst-issue #5) — a shape audit of `PPEAssignment` alone may be incomplete if `EmployeeSafety.Ppe`
   is actually writing to a logically-overlapping real-world PPE record through a different table.

## Method

Direct read this phase: `FlagPPEAssignmentForReplacementCommandValidator.cs`,
`FlagPPEInventoryForReplacementCommandValidator.cs`, `PPEAssignmentDapperRepository.cs` and
`PPEInventoryDapperRepository.cs` (both in full for their `AddString` call sites).

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PPEAssignment.Create | Yes — `JobRole`, `HazardZone`, `AssignedPpeItems` present (400 each) | Not individually re-verified | Structurally possible, no CHECK found | No — not confirmed smaller than 400 | Directly verified (param sizes) | |
| PPEAssignment.Assign | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PPEAssignment.RecordFitTest | Field set not re-opened (`Notes` per validator) | Not verified | Same | Not individually re-verified | Directly verified (validator existence); pattern-inferred (param) | Worst-issue #3 |
| PPEAssignment.FlagForReplacement | Yes — `ReplacementReason` present | Not verified | Same | **Yes — confirmed** (`Reason` 1000 validator vs `ReplacementReason` 400 param) | Directly verified | Worst-issue #1 |
| PPEAssignment.Retire | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PPEAssignment.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| PPEInventory.Create | Yes — `PpeItemCode` present (400) | Not individually re-verified | Same | No — not confirmed smaller than 400 | Directly verified (param size) | |
| PPEInventory.IssueUnits | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PPEInventory.ReceiveStock | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PPEInventory.FlagForReplacement | **No — `Reason` captured/validated, never persisted to any column** | N/A | N/A | N/A (field never reaches a string param) | Directly verified | Worst-issue #2 |
| PPEInventory.Retire | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PPEInventory.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| PPECategory.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | |
| PPETrainingFit.Create only | Yes — `TrainingType` present (400) | Not individually re-verified | N/A | Not individually re-verified | Directly verified (param size) | Dormant Update proc (inherited) |
| PPEZoneMapping.Create only | Yes — `RequiredPpeCategories` present (400) | Not individually re-verified | N/A | Not individually re-verified | Directly verified (param size) | Dormant Update proc (inherited) |
