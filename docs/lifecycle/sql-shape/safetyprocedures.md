# SQL Shape Audit — SafetyProcedures

Phase 6 of 10. Covers all 8 sub-aggregates (`EmergencyResponsePlanning`, `ErgonomicsWorkstationDesign`,
`HazardCommunicationRightToKnow`, `LotoProcedures`, `MachineGuardingInspections`, `MaterialHandlingStorage`,
`PpePolicies`, `RoutineSafetyAuditsIncidentReporting`) — the largest single newly-discovered cluster.
Same `AddAggregateProperty` mechanism as `machinesafety.md`.

## Worst issues in this module

1. **Zero of this module's 35 validator files contain any `MaximumLength` check** (confirmed by grep —
   the same 0-count as MachineSafety, and the largest raw transition count exposed to this gap in the
   solution: 6 of 8 aggregates use a 6-transition document lifecycle, 2 use a 4-transition inspection
   cycle, ~44 total transitions with zero string-length validation anywhere upstream).
2. **Same mitigation applies as MachineSafety/EmployeeSafety**: `AddAggregateProperty`'s unconditionally-
   4000 default exceeds this module's widest confirmed column (2000, from the combined 4-module grep),
   so the practical failure mode is a hard SQL truncation exception on oversized input, not silent data
   loss — but with **zero validators anywhere in this module**, every single one of its ~44 transitions
   is exposed to that reliability gap, the highest concentration of any module in this phase's census.
3. **6 of 8 aggregates share one "document lifecycle" shape** (Draft→SubmitForApproval→Publish→Retire,
   inherited) and **2 share an "inspection cycle" shape** (Conduct/Execute→BeginCorrectiveAction→Close) —
   a genuine, consistent design pattern, not an inconsistency; worth noting because it means this
   module's shape risk is concentrated in ~6 repeated field names (`Title`-ish document name,
   `Description`, `RegulatoryReference`/similar) appearing across 6 near-identical aggregates, rather than
   8 independently-varying shapes.
4. **`LotoProcedures` is the 3rd LOTO-named aggregate in the solution** (alongside `EmployeeSafety.
   LotoPrograms`, `MachineSafety.LotoSystems`) — a legitimately distinct bounded-context view of the same
   physical lockout/tagout procedure, not confirmed as a duplicate, but worth flagging that any future
   consolidation effort touching "LOTO" data would need to reconcile 3 independently-shaped tables.
5. **No CHECK constraint found on any of this module's 8 `Status` columns.**

## Method

Not individually re-opened this phase beyond the shared mechanism (verified in full for
`machinesafety.md`) and the grep confirming 0/35 validator files in this module contain `MaximumLength`.
Per-transition field-set/nullability are **pattern-inferred** from the uniform `_Create`/`_Update` SQL
convention Phase 1 confirmed for all 8 aggregates by exact-prefix match.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| EmergencyResponsePlanning.Create / Publish / RecordDrill / Retire / ScheduleDrill / SubmitForApproval | Pattern-inferred | Pattern-inferred | Structurally possible, no CHECK found | **No (mitigated)** — 4000-char default vs ≤2000 columns; zero validators means an over-2000 field throws a hard SQL error with no earlier 400 | Pattern-inferred | Document-lifecycle shape |
| ErgonomicsWorkstationDesign.Create / Publish / RecordAssessment / Retire / ScheduleAssessment / SubmitForApproval | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Document-lifecycle shape |
| HazardCommunicationRightToKnow.Create / Publish / Retire / ScheduleRevision / SubmitForApproval | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Document-lifecycle shape, no mid-cycle `RecordX` step (only 5 transitions, not 6) |
| LotoProcedures.Create / Publish / RecordAnnualAudit / Retire / ScheduleRevision / SubmitForApproval | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #4 |
| MachineGuardingInspections.BeginCorrectiveAction / Close / Create / ExecuteInspection | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Inspection-cycle shape |
| MaterialHandlingStorage.Create / FlagObstruction / Publish / ResolveObstruction / Retire / SubmitForApproval | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Document-lifecycle shape (with an added Flag/Resolve pair) |
| PpePolicies.Create / Publish / RecordEnforcementAudit / Retire / ScheduleRevision / SubmitForApproval | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | 3rd PPE-adjacent aggregate (governs the policy document, distinct from physical PPE units) |
| RoutineSafetyAuditsIncidentReporting.BeginCorrectiveAction / Close / Conduct / Create | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Inspection-cycle shape; 4th Auditing-adjacent aggregate in the solution |
| All 8 aggregates — Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified (inherited method census) | |
