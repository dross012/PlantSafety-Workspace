# Compliance Module Playbook (remaining 7 sub-aggregates)

Phase 4 domain-modernization pass, tenth module batch. Status as of 2026-08-02.
Completes the `Compliance` module: `Compliance.Permits` was modernized in an earlier pass (see [Compliance-Permits.md](Compliance-Permits.md)); this batch covers the other 7 of Compliance's 8 sub-aggregates.

## Scope: seven sub-aggregates, all the same starting state

`Auditing`, `Certification`, `Compliance` (self-named), `ManagementOfChange`, `Policy`, `Procedures`, `RegulatoryCalendar` — all had full Domain-layer DDD scaffolding (Aggregates/Enums/Events/Exceptions/Factories/Interfaces/Policies/Rules/Snapshots/Specifications/ValueObjects) with only a `Create` factory, zero Application-layer commands, and zero top-level read queries (only unrelated category read-slice sub-folders existed per aggregate, e.g. `RegulatoryCalendar`'s `Deadlines`/`Inspections`/`Renewals`). All reused the identical generic `Created→Active→Completed→Closed→Archived` status enum and `Can*` guard scaffolding, unchanged, with domain-appropriate method names layered on top.

## Domain models built this pass

- **Auditing**: Created → **Start** → Active → **RecordFindings** → Completed → **Close** → Closed → **Archive** → Archived.
- **Certification**: Created → **Verify** → Active → **Expire**/renewal loop (**Renew**, same-status) → Completed → **Close** → Closed → **Archive** → Archived.
- **Compliance** (self-named aggregate, `PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance`): Created → **Activate** → Active → **Evaluate** (same-status) → **Complete** → Completed → **Close** → Closed → **Archive** → Archived.
- **ManagementOfChange**: Created → **Approve** → Active → **Implement** → Completed → **Close** → Closed → **Archive** → Archived.
- **Policy**: Created → **Approve** → Active → **RecordAcknowledgment** (same-status) / **Supersede** → Completed → **Close** → Closed → **Archive** → Archived.
- **Procedures**: Created → **Approve** → Active → **Supersede** → Completed → **Close** → Closed → **Archive** → Archived.
- **RegulatoryCalendar**: Created → **Activate** → Active → **RecordCompliance** → Completed → **Close** → Closed → **Archive** → Archived.

## The self-named `Compliance` aggregate collision

`Compliance.Compliance.Aggregates.Compliance` — a class named identically to the module and to the word used pervasively everywhere else in the codebase. Resolved with fully-qualified/aliased references (`using ComplianceAggregate = PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance;`) everywhere outside its own Domain files, plus consistently distinct naming for every generated artifact: write repository `ComplianceRuleAggregateDapperRepository`, read repository `ComplianceRuleReadDapperRepository`, table `[Compliance].[ComplianceRuleAggregate]`, procs `usp_ComplianceRuleAggregate_*`, domain events prefixed `ComplianceRule*` (not bare `Compliance*`), DI extension `AddComplianceRuleInfrastructure`. No ambiguous-reference compile errors resulted.

## Execution: three parallel agents, seven aggregates, zero manual fixes needed

1. **Auditing + Certification + Compliance** (self-named) — full build from Create-only scaffolds, including the collision-handling above. Found and worked around an unrelated second `AuditingDapperRepository` already existing in `PlantSafety.Infrastructure.Integrations.Auditing.Persistence` (a different, live, unrelated feature) — renamed the new write repo to `AuditingAggregateDapperRepository`/read to `AuditingReadDapperRepository` to avoid the clash.
2. **ManagementOfChange + Policy + Procedures** — full build from Create-only scaffolds. Policy/Procedures SQL tables named `PolicyAggregate.sql`/`ProceduresAggregate.sql` to stay distinct from the many pre-existing `Policy*`/`Procedures*` category tables in the same schema, while procs kept the shorter `usp_Policy_*`/`usp_Procedures_*` naming.
3. **RegulatoryCalendar** — full build from Create-only scaffold, explicitly scoped to leave the `Deadlines`/`Inspections`/`Renewals` read-only category sub-folders untouched.

All three agents' work integrated cleanly with the shared `InfrastructureServiceRegistration.cs` file (each added its own `.Add*Infrastructure()` call additively; verified after all three landed that all 7 calls coexist with no lost edits).

A reduced-scope deviation applied to the Auditing/Certification/Compliance batch only, per the invocation: single by-id read query per aggregate (no range/list query), and `ListAsync` on the write-side Dapper repositories returns an empty list rather than a fabricated bulk proc — smaller, reversible choice, consistent with the `Compliance.Permits` precedent.

## A second recurring dead-scaffold pattern, same as every prior module

All 7 sub-aggregates had pre-existing, unregistered, non-functional `<Name>Repository.cs`/`<Name>ReadModel.cs` pairs referencing stored procedures that never previously existed (`AuditingRepository`/`AuditingReadModel`, `CertificationRepository`/`CertificationReadModel`, `ComplianceRepository`/`ComplianceReadModel`, `ManagementOfChangeRepository`/`ManagementOfChangeReadModel`, `PolicyRepository`/`PolicyReadModel`, `ProceduresRepository`/`ProceduresReadModel`, `RegulatoryCalendarRepository`/`RegulatoryCalendarReadModel`) — same pattern as `Compliance.Permits`'s `PermitsRepository`/`PermitsReadModel` and the `SafetyEvents`/`Analytics` orphans found earlier. All three agents correctly avoided colliding with these (distinct naming) but left deletion out of scope. Verified zero live references (no DI registration, no instantiation, no usage anywhere in `Infrastructure/DependencyInjection`, `Api`, or `Application`) and deleted all 14 files directly after the independent verification pass.

One nuance found during verification for the `ManagementOfChange` batch: my new `usp_ManagementOfChange_GetById`/`usp_Policy_GetById`/`usp_Procedures_GetById` procs happen to share names with what the orphaned repositories expected, making those orphans *functionally callable* as a side effect — but they remained unregistered in DI and were deleted anyway since nothing referenced them.

## Verified independently (not just trusting agent self-reports)

- All 7 write-side repositories: `outboxWriter.Enqueue` present exactly twice each (Add + Update) — confirmed via grep after locating each repository's actual folder (note: `Compliance`'s self-named aggregate lives at `Compliance/Compliance/Persistence/`, and `Policy`'s aggregate repository lives under `Compliance/Policies/` — plural — not `Compliance/Policy/`, since the singular folder is occupied by the pre-existing category sub-features).
- All 7: `ExecuteScalarAsync` (12 files) + `ConcurrencyConflictException` (6 files) present in Infrastructure.
- 38 files use `ErrorSeverity.NotFound`; 31 have the `ConcurrencyConflictException` passthrough across this batch's Application layer.
- All 7 DI registration calls (`AddComplianceAuditingInfrastructure`, `AddComplianceCertificationInfrastructure`, `AddComplianceRuleInfrastructure`, `AddManagementOfChangeAggregateInfrastructure`, `AddPolicyAggregateInfrastructure`, `AddProceduresAggregateInfrastructure`, `AddRegulatoryCalendarInfrastructure`) confirmed present and coexisting in the central `InfrastructureServiceRegistration.cs`, alongside all previously-registered modules with no lost entries.
- Full solution build (`Api`, `Tests`, `IntegrationTests`) clean both before and after the orphan-file deletion.
- 858/858 unit tests passing, verified with a fresh independent run both before and after deleting the 14 orphaned files.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results across any of the 7 sub-aggregates (same open item as every prior module).
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for any of the newly built write paths.
- Auditing/Certification/Compliance (self-named) only have a by-id read query, no list/range query — deliberate reduced scope, not a defect, but worth revisiting if a list view is ever needed.
- The many read-only category sub-features under each of these 7 aggregates (e.g. `RegulatoryCalendar`'s `Deadlines`/`Inspections`/`Renewals`, `Policy`'s `Approvals`/`ChemicalHandling`/`Contractor`/`Documents`/`Emergency`/`Environmental`/`Equipment`/`HR`/`PPE`/`Revisions`/`SafetyPolicies`, `Certification`'s `DOT`/`EPA`/`ISO14001`/`ISO45001`/`IndustrySpecific`) were not audited this pass — same open question carried forward from every earlier module in this effort.

## Compliance module: now fully modernized

With this batch, all 8 of Compliance's sub-aggregates (`Permits` + these 7) have real guarded lifecycles, tenant-scoped ETag-aware write paths, and NotFound-mapped read queries. This closes out the `Compliance` module entirely.
