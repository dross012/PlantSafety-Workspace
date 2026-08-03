# SQL Shape Audit — Scheduling

Phase 6 of 10. Covers `PMScheduling`, `WorkOrderPriority` (rich); `EmergencyDrillPlanning`,
`HazardIsolationSequencing`, `ShiftFatigue` (create-only).

## Worst issues in this module

1. **`WorkOrderPriority.Escalate`'s `EscalationReason` is validated (up to 1000 chars) but never
   persisted at all** — `EscalateWorkOrderPriorityCommandValidator.cs:22`: `MaximumLength(1000)`.
   `WorkOrderPriorityDapperRepository.cs` (read in full for this field) writes `EscalatedAtUtc`
   (a timestamp) on both the Insert and Update paths but **contains no `AddString("EscalationReason", ...)`
   call and no `EscalationReason` column in the row/hydration shape at all**. Same "field silently
   dropped before persistence" pattern as `SafetyPolicies.Reject`/`Retire`'s `Reason` — the reason a work
   order was escalated survives only in `WorkOrderPriorityEscalatedEvent` (per F15, which already flags
   this event as carrying `EscalationReason` but not the prior-priority value; this phase adds that the
   *column itself doesn't exist to check against*, a step worse than F15 assumed). Directly verified.
2. **`RecordPMSchedulingCompletion.CompletionNotes` — confirmed truncation risk, largest validator/param
   gap ratio found this phase (5x).** `RecordPMSchedulingCompletionCommandValidator.cs:22`:
   `MaximumLength(2000)`. `PMSchedulingDapperRepository.cs:90,127`: `AddString("CompletionNotes",
   aggregate.CompletionNotes, 400)`. A completion note between 401 and 2000 characters passes validation
   and is silently cut to 400. Directly verified.
3. **`ResolveWorkOrderPriority.ResolutionNotes` — same bug, same module, same shape.**
   `ResolveWorkOrderPriorityCommandValidator.cs:22`: `MaximumLength(2000)`.
   `WorkOrderPriorityDapperRepository.cs:88,123`: `AddString("ResolutionNotes", aggregate.ResolutionNotes,
   400)`. Directly verified. **This module now has 2 of the 3 largest truncation-risk gaps found in the
   entire audit** (both 2000-vs-400, a 1600-char gap — larger than PermitToWork's 1000-vs-400 and far
   larger than the 256-vs-200 pattern found in 4 other modules).
4. **`PMScheduling.Cancel` remains the one Cancel method in the entire solution, confirmed unreachable
   from any HTTP route** (inherited headline finding) — fully guarded, SQL-persistable, dead from the
   API's perspective.
5. **No CHECK constraint found on either aggregate's status columns.**

## Method

Direct read this phase: `EscalateWorkOrderPriorityCommandValidator.cs`,
`RecordPMSchedulingCompletionCommandValidator.cs`, `ResolveWorkOrderPriorityCommandValidator.cs`,
`PMSchedulingDapperRepository.cs` and `WorkOrderPriorityDapperRepository.cs` (both in full for the fields
above, including read-side row shapes to confirm `EscalationReason`'s absence is total, not just
missed by an incomplete grep).

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PMScheduling.Create (Draft) | Field set not re-opened this phase | Not verified | Structurally possible, no CHECK found | Not individually re-verified | Pattern-inferred | |
| PMScheduling.SchedulePreventiveMaintenance | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PMScheduling.RecordCompletion | Yes — `CompletionNotes` present | Not verified | Same | **Yes — confirmed** (2000 validator vs 400 param) | Directly verified | Worst-issue #2 |
| PMScheduling.Escalate | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | Distinct from `WorkOrderPriority.Escalate` below — not confirmed if this shares the same dropped-field bug |
| PMScheduling.Cancel | Guarded correctly, field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (guard, unreachability, inherited) | Worst-issue #4 |
| WorkOrderPriority.Create | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| WorkOrderPriority.Escalate | **No — `EscalationReason` captured/validated, never persisted to any column** | N/A | N/A | N/A (field never reaches a string param) | Directly verified | Worst-issue #1 |
| WorkOrderPriority.Resolve | Yes — `ResolutionNotes` present | Not verified | Same | **Yes — confirmed** (2000 validator vs 400 param) | Directly verified | Worst-issue #3 |
| WorkOrderPriority.StartWork | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| WorkOrderPriority.Close | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| WorkOrderPriority.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| EmergencyDrillPlanning.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | |
| HazardIsolationSequencing.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | |
| ShiftFatigue.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | |
