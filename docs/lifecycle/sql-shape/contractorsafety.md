# SQL Shape Audit — ContractorSafety

Phase 6 of 10. Covers all 5 rich sub-aggregates (`ActiveMonitoringAuditing`,
`IncidentTrackingPerformanceEvaluation`, `PermitToWork` [ContractorSafety-scoped], `PreQualificationVetting`,
`SiteOrientation`) plus the create-only `ContractorVisitor`. Same `AddAggregateProperty` mechanism as
`machinesafety.md`.

## Worst issues in this module

1. **Only 4 of 18 validator files in this module contain any `MaximumLength` check** — better than
   MachineSafety/SafetyProcedures' zero, but the majority of this module's ~19 transitions still have no
   upstream string-length validation.
2. **Same truncation-risk mitigation applies**: `AddAggregateProperty`'s 4000-char default exceeds this
   module's confirmed max column width (2000, combined 4-module grep) — oversized input throws a hard
   SQL error rather than truncating silently, for the 14 of 18 commands lacking any validator.
3. **`ContractorSafety.PermitToWork` is a 3rd, flagged-not-confirmed overlapping concept with
   `PermitToWork.PermitWorkflow`** (inherited) — if these are genuinely the same real-world permit being
   modeled twice, a shape audit of one without reconciling against the other risks describing only half
   of the actual data-integrity picture (e.g., could the same physical permit number end up in both
   tables with different lifecycle states?). Not resolved this phase.
4. **`IncidentTrackingPerformanceEvaluation` has no `Close`/`Resolve`** (inherited) — same "raised but
   never formally closed" shape as `Observability.MachineProcessMonitoring`.
5. **`ContractorVisitor` (Create-only) — SQL destination not individually re-confirmed this phase**
   (inherited "Unconfirmed" status from `module-inventory/contractorsafety.md`); listed again in
   `emerging-createonly.md` as one of the 11 (now 13, see that file) zero-confirmed-SQL orphans.

## Method

Not individually re-opened this phase beyond the shared mechanism (verified in full for
`machinesafety.md`) and a grep confirming 4/18 validator files in this module contain `MaximumLength`.
Field-set/nullability per transition are **pattern-inferred** from the uniform `_Create`/`_Update` SQL
convention Phase 1 confirmed for all 5 rich aggregates by exact-prefix match.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| ActiveMonitoringAuditing.Create / RecordObservation / Resolve / ScheduleFollowUp | Pattern-inferred | Pattern-inferred | Structurally possible, no CHECK found | **No (mitigated)** — same 4000-vs-≤2000 reasoning | Pattern-inferred | This is one of the aggregates whose validators do exist (`RecordObservationActiveMonitoringAuditingCommandValidator.cs`, `ResolveActiveMonitoringAuditingCommandValidator.cs`, `ScheduleFollowUpActiveMonitoringAuditingCommandValidator.cs` — 3 of the module's 4 files with `MaximumLength`) |
| IncidentTrackingPerformanceEvaluation.Create / BeginInvestigation / DetermineEligibility / DocumentCapa | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #4 — no formal Close/Resolve |
| PermitToWork(ContractorSafety).Create / RecordHazardAssessment / Issue / Suspend / Expire / Close | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Worst-issue #3 — possible duplicate concept with `PermitToWork.PermitWorkflow` |
| PreQualificationVetting.Create / Approve / BeginReview / Expire / Reject | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | Approve/Reject pair present, same "positive undo path" shape as `SafetyPolicy` |
| SiteOrientation.Create / DeliverOrientation / Expire / VerifyComprehension | Pattern-inferred | Pattern-inferred | Same | **No (mitigated)** | Pattern-inferred | |
| ContractorVisitor.Create only | Not individually re-verified | Not verified | N/A | Unconfirmed — SQL destination itself unconfirmed (see `emerging-createonly.md`) | Directly verified (method presence only) | If no SQL destination exists at all, the truncation-risk question is moot — the write would fail/no-op before any string ever reaches a parameter |
| All 5 rich aggregates — Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified (inherited method census) | |
