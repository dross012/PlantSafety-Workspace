# SQL Shape Audit — Compliance

Phase 6 of 10. Covers the 8 Create-only aggregates under `PlantSafety.Domain.Compliance`:
`Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`, `Procedures`,
`RegulatoryCalendar`, and the `Auditing` marker (duplicate of the live `Auditing.AuditEngagement`/
`AuditFinding` — see `auditing.md`). Per `module-inventory/compliance.md` and F13
(`command-handler-map/_shared-findings.md`), this is already the single largest concentration of
"Create with no confirmed write proc" aggregates in the solution. This phase's contribution is a
**direct DI-implementation check** (grep of each aggregate's fully-qualified domain-interface
namespace against the whole solution) — the same check that produced F13 for `RegulatoryCalendar` —
run this phase against the other 7 aggregates in this cluster for the first time.

## Worst issues in this module

1. **[NEW THIS PHASE] All 8 of this module's domain write-repository interfaces have zero
   implementations anywhere in the solution — not just `RegulatoryCalendar`.** Direct grep of
   `PlantSafety.Domain.Compliance.{Certification,Compliance,ManagementOfChange,Permits,Policy,
   Procedures,Auditing}.Interfaces` (7 namespaces, `RegulatoryCalendar` already confirmed by F13) found,
   for every one, only the interface file and its paired `I<Aggregate>Factory.cs` referencing the
   namespace — no implementing class anywhere. This means F13's "worse than no stored procedure — the
   C# write-repository doesn't even exist" finding, previously scoped to 1 aggregate, actually describes
   **all 8 aggregates in this module**. Every one of them would fail at DI-container resolution (missing
   registration) before a `Create` call could reach the SQL layer at all, regardless of whatever stored
   procedure name the handler might target.
2. **A same-named-but-unrelated trap exists for `Procedures`**, mirroring Analytics' `Culture` trap
   (`analytics.md` #2): `PlantSafety.Infrastructure/Auditing/AuditEngagement/Procedures/Persistence/
   ProceduresDapperRepository.cs` does implement an `IProceduresRepository` — but it is a **different**
   interface (`PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories.
   IProceduresRepository`, a read-model interface scoped to Auditing's own "Procedures" sub-concept),
   not `PlantSafety.Domain.Compliance.Procedures.Interfaces.IProceduresRepository` (the aggregate this
   file audits). A short-name-only search would wrongly conclude `Compliance.Procedures` has a working
   repository. Directly verified by comparing both files' full namespaces.
3. **`Policy` has the largest SQL footprint of any create-only aggregate in the solution (12
   `usp_Policy*` procs) and still has neither a matching write proc nor an implementing repository
   class** (inherited SQL census + this phase's DI check) — the two gaps compound: even if one of the 12
   procs turned out to be a plausible target, there is still no class to call it from.
4. **`RegulatoryCalendar` remains the single sharpest finding in this cluster** (F13, inherited) — its
   read-only, same-named `RegulatoryCalendarRepository` class calls 2 procs
   (`usp_RegulatoryCalendar_GetById`/`_GetAll`) that don't exist as `.sql` files anywhere, compounding
   the missing-write-repository problem with a missing-read-SQL problem on the adjacent class.
5. **All 8 aggregates' `*CreatedEvent`s are nonetheless written to Outbox as soon as `AddDomainEvent`
   runs inside the aggregate's `Create` factory method** (F7) — the event's existence in Outbox says
   nothing about whether persistence succeeded, and per worst-issue #1 above, for every aggregate in
   this module persistence cannot succeed via the domain interface as it stands today.

## Method

As with `analytics.md`, field-set/nullability/enum-drift comparison against a stored-procedure body is
not applicable for any of the 8 aggregates — none has a confirmed genuine write proc (inherited
file-name census) and, per this phase's new DI-implementation check, none has an implementing
repository class either. The table below records the DI/SQL failure point for each aggregate instead.

| Aggregate | Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Certification | Create | N/A — no implementing class (`ICertificationRepository`, 0 implementations) | N/A | N/A | N/A — fails at DI before any parameter binds | Directly verified (interface-implementation grep, this phase) | |
| Compliance | Create | N/A — no implementing class (`IComplianceRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | `usp_ComplianceResources_BulkUpdate` remains a plausible-but-unconfirmed SQL target even if a repository existed (inherited) |
| ManagementOfChange | Create | N/A — no implementing class (`IManagementOfChangeRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | Also cross-listed in `emerging-createonly.md` |
| Permits (Compliance) | Create | N/A — no implementing class (`IPermitsRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | 6th "permit"-named concept in the solution (inherited); this one has neither SQL nor a repository |
| Policy | Create | N/A — no implementing class (`IPolicyRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | Worst-issue #3 — largest SQL footprint, still no path |
| Procedures | Create | N/A — no implementing class for the correct (`Domain.Compliance.Procedures`) interface | N/A | N/A | N/A | Directly verified (this phase) | Worst-issue #2 — do not confuse with the unrelated Auditing-scoped `ProceduresDapperRepository` |
| RegulatoryCalendar | Create | N/A — `IRegulatoryCalendarRepository`, 0 implementations (F13, inherited) | N/A | N/A | N/A | Directly verified (inherited F13) | Sharpest finding in the cluster — also has 2 missing `.sql` files on its unrelated read-only sibling class |
| Auditing (Compliance marker) | Create | N/A — no implementing class (`IAuditingRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | Duplicate of the live, fully-functional `Auditing.AuditEngagement`/`AuditFinding` (see `auditing.md`) — do not confuse |
| All 8 above | Update/Delete/Cancel/status | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified (inherited) | |
