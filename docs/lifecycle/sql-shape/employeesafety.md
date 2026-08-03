# SQL Shape Audit — EmployeeSafety

Phase 6 of 10. Covers all 7 sub-aggregates (`EmergencyPreparedness`, `EnvironmentalHealthMonitoring`,
`ErgonomicsAndMaterialHandling`, `HazardCommunicationTraining`, `LotoPrograms`, `MachineGuarding`, `Ppe`),
entirely new to this phase's tracing. Same `AddAggregateProperty` persistence mechanism as
`machinesafety.md` — that file's Method section and worst-issue #2 mechanism explanation apply
identically here and are not re-derived in full.

## Worst issues in this module

1. **Only 8 of 28 validator files in this module contain any `MaximumLength` check at all** (confirmed
   by grep) — better than MachineSafety/SafetyProcedures' zero, but still a minority; the other 20
   commands' free-text fields have no upstream length validation.
2. **Same truncation-risk mitigation as MachineSafety applies**: this module's repositories use
   `AddAggregateProperty` (default `stringSize=4000`, never overridden), and the widest `NVARCHAR` column
   confirmed across `EmployeeSafety/Sql/Tables/*.sql` (combined grep with the other 3
   `AddAggregateProperty`-using modules) is 2000 — so oversized input throws a hard SQL error rather than
   truncating silently. The 20 commands lacking a validator are exposed to that reliability gap (an
   unhandled 500 on a >2000-char field), not to silent data loss.
3. **`EnvironmentalHealthMonitoring` has the thinnest guard density of this cluster** (inherited,
   `module-inventory/employeesafety.md`) — only 1 guard site found across 4 methods (`Create`/
   `BeginCorrectiveAction`/`RecordMeasurement`/`ResolveExceedance`), flagged for a full read to confirm
   the other 3 transitions have real preconditions.
4. **`LotoPrograms` and `MachineGuarding` have no plain `Create` method name** (inherited) — `LotoPrograms`
   presumably creates via a differently-named factory, `MachineGuarding` likely via `Install`; not
   confirmed this phase which method is the actual entry point, meaning this file's "Create" row for
   both is a **best-guess placeholder**, not a directly observed transition name.
5. **`Ppe` (EmployeeSafety) closely mirrors `PPE.PPEAssignment`'s method names** (`RecordFitTest`,
   `FlagForReplacement`, `Retire`) — a possible undocumented duplicate/overlap, not confirmed; if both
   write to conceptually the same "PPE unit," a shape audit of one without the other risks missing which
   one is actually authoritative. Flagged, not resolved this phase.

## Method

Not individually re-opened this phase beyond the shared `AddAggregateProperty`/`StoredProcedureBuilder`
mechanism (verified in full for `machinesafety.md`) and a grep of this module's 28 validator files for
`MaximumLength` (8/28 have it) plus a combined grep of `NVARCHAR` column widths across this module and
its 3 siblings (max 2000). Field-set/nullability per individual transition are **pattern-inferred** from
the uniform `_Create`/`_GetAll`/`_GetById`/`_GetItems`/`_Update` SQL convention Phase 1 confirmed present
for all 7 aggregates by exact-prefix match — no individual repository or table file in this module was
opened in full this phase.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| EmergencyPreparedness.Create / ConductDrill / RecordReadiness / ScheduleDrill | Pattern-inferred (uniform shape) | Pattern-inferred | Structurally possible, no CHECK found in this module (none of its tables appeared in the 4-file solution-wide CHECK census) | **No (mitigated)**, same 4000-vs-≤2000 reasoning as MachineSafety | Pattern-inferred | This module's validator has `MaximumLength` present per the 8/28 count — not confirmed if this specific command is one of the 8 |
| EnvironmentalHealthMonitoring.Create / BeginCorrectiveAction / RecordMeasurement / ResolveExceedance | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #3 — thinnest guard density, top Phase 2/7 candidate for a full read |
| ErgonomicsAndMaterialHandling.Create / Close / ImplementControls / RecordAssessment / ScheduleFollowUp | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Has a genuine `Close` — one of few in this cluster with a formal terminal transition |
| HazardCommunicationTraining.Create / Close / DeliverTraining / ScheduleNextCycle / VerifyComprehension | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| LotoPrograms.(Create-equivalent) / ApplyIsolation / Document / RecordAnnualAudit / Release / VerifyZeroEnergy | Pattern-inferred; **entry-point method name unconfirmed** | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #4 |
| MachineGuarding.FlagBypass / Install(-as-Create?) / RecordInspectionResult / Retire / ScheduleCorrectiveAction / ScheduleInspection / VerifyCorrectiveAction | Pattern-inferred; **entry-point method name unconfirmed** | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Richest single lifecycle in this new-this-phase population (7 methods) |
| Ppe.Create / FlagForReplacement / RecordFitTest / RecordInspection / Retire | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #5 — possible duplicate of `PPE.PPEAssignment`, see `ppe.md` |
| All 7 aggregates — Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified (inherited method census) | |
