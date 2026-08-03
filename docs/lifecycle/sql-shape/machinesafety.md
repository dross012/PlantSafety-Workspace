# SQL Shape Audit — MachineSafety

Phase 6 of 10. Covers all 6 sub-aggregates (`EmergencyStopDevices`, `LotoSystems`, `MachineGuardsBarriers`,
`PresenceSensingDevices`, `SafetyValvesRelays`, `TwoHandControls`), entirely new to this phase's tracing
(the prior Ingestion Discovery never opened this cluster). All 6 share one persistence mechanism,
confirmed identical by direct read of one full repository plus grep-confirmed call-site uniformity
across the other 5.

## Worst issues in this module

1. **Zero FluentValidation `MaximumLength` checks exist anywhere in this module's 24 validator files** —
   confirmed by direct grep (`grep -l MaximumLength PlantSafety.Application/MachineSafety/Validation/*.cs`
   → 0 of 24 files). Every free-text field on every one of this module's ~30 transitions (across 6
   aggregates) reaches the persistence layer with no length check of any kind upstream.
2. **...but this module uses `AddAggregateProperty`, not raw `AddString`, and its default `stringSize`
   parameter (4000) is never overridden at any of this module's call sites** (confirmed: 0 of the
   solution-wide 734 `AddAggregateProperty` calls pass an explicit size argument). Confirmed this
   module's widest table column is `NVARCHAR(2000)` (`grep`-ed all 6 `Sql/Tables/*.sql` files) — **the
   4000-char parameter is always wider than the actual column**, so a caller-supplied string longer than
   the real column limit is **not truncated client-side**; it reaches SQL Server intact and the
   `INSERT`/`UPDATE` throws a hard `String or binary data would be truncated` error. **This is the
   opposite failure mode from the classic `AddString`-with-small-explicit-size bug**: not silent data
   loss, but an unhandled 500 on any oversized input, for a module with zero validators to catch it
   earlier and return a clean 400 instead. Directly verified (`StoredProcedureBuilderAggregateExtensions.
   cs` read in full; `EmergencyStopDevicesDapperRepository.cs` read in full as the representative sample;
   column widths grepped across all 6 tables).
3. **`Retire` functions as the de facto terminal transition across all 6 aggregates** (inherited) — no
   `Archive`/`Cancel`/`Delete` exists anywhere in this cluster, a more internally consistent naming
   choice than most of the rest of the solution.
4. **No CHECK constraints found on any `Status` column in this module's 6 tables** — same structural
   enum-drift exposure as everywhere outside Alert/Dashboard/Widget; not individually re-verified per
   aggregate this phase beyond the one table read in full (`EmergencyStopDevices.sql`).
5. **AuditLog coverage: N/A for all 6** — none of the 6 aggregates in this module appear in F8's
   23-aggregate AuditLog list (inherited).

## Method

Direct read this phase: `StoredProcedureBuilderAggregateExtensions.cs` (in full, the shared extension all
26 EmployeeSafety/MachineSafety/SafetyProcedures/ContractorSafety repositories depend on),
`EmergencyStopDevicesDapperRepository.cs` (in full, representative), `EmergencyStopDevices.sql` (table
DDL, in full). Confirmed by grep: all 6 repositories in this module call `AddAggregateProperty` (not raw
`AddString`) for every property; zero of the module's 24 validator files contain `MaximumLength`; the
widest `NVARCHAR` size across all 6 tables in `MachineSafety/Sql/Tables/*.sql` is 2000. The other 5
aggregates' per-field shapes are **pattern-inferred** from this one directly-verified sample plus the
uniform `_Create`/`_Update`/column-naming convention confirmed by Phase 1's exact-prefix SQL census — not
individually opened this phase.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| EmergencyStopDevices.Create | Yes — every aggregate property has a matching `AddAggregateProperty` call (`DeviceTitle`, `DeviceType`, `EquipmentLocation`, `TestedByUserId`, etc.), directly verified | Yes — nullable columns (`SiteId`, `TestedAtUtc`, `NextTestDueAtUtc`, `ResponseTimeScore`) map to nullable C# properties; not-null columns map to non-nullable properties, consistent | Structurally possible on `Status` (no CHECK) | **No (mitigated)** — 4000-char default param exceeds the 400-char `DeviceTitle`/`EquipmentLocation` columns; an over-limit value throws a hard SQL error instead of truncating silently | Directly verified | Worst-issue #1/#2 |
| EmergencyStopDevices.BeginRepair / RecordTestResult / Retire / ScheduleTest | Pattern-inferred same shape as Create (all route through the same `AddAggregateProperty`-based `UpdateAsync`) | Pattern-inferred consistent | Same | **No (mitigated)**, same reasoning | Pattern-inferred | 0/24 module validators cover any of these transitions' fields |
| LotoSystems.Create / ApplyIsolation / RecordAnnualAudit / Release / VerifyZeroEnergy | Pattern-inferred (uniform `_Create`/`_Update` shape, not individually opened) | Pattern-inferred | Same | **No (mitigated)**, same reasoning (module max column width 2000 < 4000 default) | Pattern-inferred | |
| MachineGuardsBarriers.Create / BeginCorrectiveAction / RecordInspectionResult / Retire / ScheduleInspection / VerifyCorrectiveAction | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| PresenceSensingDevices.Create / RecordCalibrationResult / RecordTriggerEvent / Retire / ScheduleCalibration | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| SafetyValvesRelays.Create / BeginRepair / RecordInspectionResult / Retire / ScheduleInspection | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| TwoHandControls.Create / RecordTestResult / Retire / ScheduleTest | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| All 6 aggregates — Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
