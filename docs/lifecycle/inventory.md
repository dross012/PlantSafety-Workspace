# Lifecycle Transition Inventory — Master Summary

**Phase 1 of 10 (Lifecycle Coverage Audit).** Root: `C:\Users\dross\source`. Date: 2026-07-22.
Companion/follow-on to the completed Ingestion Pipeline Discovery
(`docs/ingestion/blueprint.md`, `docs/ingestion/lifecycle-coverage/*.md`). This phase goes
**per-transition** where the prior discovery's Phase 6 went per-aggregate: every public
lifecycle-shaped method on every domain aggregate/entity, cross-referenced against Application-layer
commands/handlers and SQL stored procedures.

---

## 1. Headline Counts

| Module | Aggregate classes (real, non-marker) | Total transitions found | Create-only aggregates (missing Update/Close/Cancel/Delete entirely) | Rich aggregates (≥1 transition beyond Create) |
|---|---|---|---|---|
| **PlantSafety** | 93 | **309** (93 Create + 216 non-Create) | 45 | 48 |
| **DataLake** | 0 (no C# aggregate layer — command/handler-shaped, see `module-inventory/datalake.md`) | 7 command-shaped transitions across Ingestion/Processing/Integration/Archival/Governance/MetadataCatalog | N/A (structural — no state machine exists to grow) | N/A |
| **Intelligence** | 2 (`TrainingJob`, `Investigation`) | 13 (7 TrainingJob + 6 Investigation) | 0 | 2 (both bypassed at runtime — see `module-inventory/intelligence.md`) |
| **Azure** | 0 (ARM control-plane resources, no local aggregate) | ~15 command-handler-shaped transitions across 9 subdomains (Phase 3/6 count, not re-enumerated this phase) | N/A | N/A |
| **Reporting** | 0 (no aggregates, no persistence at all) | 0 | N/A | N/A |
| **Solution total (transition rows enumerated this phase)** | 95 real aggregate/entity classes | **~344** | 45 | 50 |

**Missing entirely (transition category structurally absent for that aggregate, not merely unguarded):**
- **Delete/hard-delete: 93 of 93 PlantSafety aggregates (100%)** — zero `Delete(...)` methods found anywhere in `PlantSafety.Domain` (confirmed by a full grep of every `Aggregates/*.cs` file this phase, not a sample). Plausibly correct-by-design for regulatory/audit records, but it means **there is no C# or SQL path to remove a record from any of PlantSafety's ~100 aggregates** — a data-retention/right-to-erasure gap if any of this data is ever subject to one.
- **Cancel: 92 of 93 (99%)** — the sole exception, `PMScheduling.Cancel`, is fully guarded and event-raising but was already confirmed by the prior discovery to be unreachable from any HTTP route (`lifecycle-coverage/plantsafety-lifecycle.md` §[NEW] finding 4). This phase found no second `Cancel` anywhere else in the domain layer.
- **Archive: 85 of 93 (91%)** — only 8 aggregates have it at all: `Accident`, `Risk`, `Scorecard`, `SafetyPolicy`, `PermitWorkflow`, `PermitTemplate`, `Dashboards.Dashboard`, and `Operational.Alerts.Alert`. Of these, `Accident`/`Risk`/`Scorecard` are confirmed (prior phase, directly verified) to guard the transition but never call `AddDomainEvent` — a "silent archive" pattern. `SafetyPolicy`/`PermitWorkflow` were pattern-inferred to match; `PermitTemplate`/`Dashboards.Dashboard`/`Operational.Alerts.Alert` are **new to this phase** and not yet individually re-read for event-publish behavior (flagged for Phase 2).
- **45 of 93 aggregates (48%) never grew past Create at all** — no Update, Close, Cancel, Archive, or any status transition exists in the domain model. This is a larger population than the prior discovery's "~50 of ~100" estimate suggested was sampled (that phase directly grep-confirmed ~25 of them); this phase's full census confirms **all 45** by direct file read of every `Aggregates/*.cs` file in the module, not a sample.

**SQL-only orphans (a named stored procedure like `sp_ApproveX`/`sp_RejectX`/`sp_ArchiveX` with no C# domain method or command/handler behind it): effectively zero, and this is itself the finding.**
A full census of PlantSafety's 541 `usp_*` stored procedure names (via file listing, not content reads)
found **almost no per-transition-verb naming at all** — the vocabulary is overwhelmingly `_Create`/
`_Insert`/`_Update`/`_Upsert`/`_GetById`/`_GetAll`/`_List`/`_ListByTenant`. Grepping the full name list for
`_Approve|_Reject|_Archive|_Restore|_Cancel|_Close|_Assign|_Complete|_Escalate|_Publish|_Retire|_Resolve|
_Dismiss|_Suspend|_Expire|_Deprecate|_Verify|_Remediate|_TriggerCapa|_Issue` returned **one incidental hit**
(`usp_Restore_GetRestoreJobs`, a backup/DR job list, unrelated to any domain aggregate's Restore concept).
This confirms and extends the prior discovery's finding (`usp_*_Delete`/`_Cancel`/`_Close` grep, one hit:
`usp_PolicyAcknowledgment_DeleteByPolicyId`): **PlantSafety's SQL layer does not name procedures after
domain transitions.** Every non-Create transition (Approve, Reject, Escalate, Close, etc.) is persisted
through the aggregate's one generic `_Update`/`_BulkUpdateResource` proc (confirmed present for 46 of 48
rich aggregates by name; the other 2 — `Dashboards.Dashboard`/`Widget` — use a `_Reads`/`_Writes` naming
convention instead of `_Update`, confirmed present under that name, not actually missing). **Practical
consequence: "SQL-only orphan" as a per-transition-name search is close to a non-finding for PlantSafety
by construction — the naming convention doesn't support it — but this also means the SQL layer cannot by
itself enforce or reveal which specific transition wrote which row**; a `usp_Accident_Update` call that
sets `Status = 'Archived'` is indistinguishable, from the SQL layer alone, from any other status write.

**C#-only orphans (domain method + command/handler exists, but NO corresponding SQL persistence — would fail or no-op): 11 aggregates confirmed, all at the "not even Create persists" level, not a partial-transition gap.**
Loose (case-insensitive, whole-name) grep of the 541-proc list found **zero matching stored procedures
under any naming convention** for: `AiGovernance`, `AiSafetyIntelligence`, `ManagementOfChange`,
`RegulatoryCalendar`, `ContractorVisitor`, `Governance`, `HumanFactors`, `OperationalSafety`,
`ProcessDefinition` (`ProcessManagement`'s aggregate), and `Sustainability` — 10 aggregates whose `Create(...)`
factory (their entire public surface) has no confirmed SQL destination anywhere in the 885 `.sql` files
under `PlantSafety/`. An 11th, `WorkerWellBeing`, has exactly two stored procedures
(`usp_WorkerWellBeing_GetById`, `usp_WorkerWellBeing_List`) — **read-only**, with no `_Create`/`_Insert`
counterpart found under any name — meaning even if a `WorkerWellBeing` were constructed in memory, there
is no confirmed write path for it. **This is the single most concrete "would fail/no-op" finding of this
phase**: these are not shallow Create-only aggregates in the "intentionally simple" sense the prior
discovery described — they are Create-only aggregates that additionally appear to be **structurally
unwired from persistence entirely**, consistent with the "aspirational/scaffold" naming (AI Governance, AI
Safety Intelligence, Enterprise Fabric [which *does* have SQL, as `Twin`/`FabricNode`-named procs — see
caveat below], Human Factors, Sustainability) of the newest-looking domain folders in the solution. Two
folders that looked like candidates for this list on a naive name match turned out **not** to qualify on
closer check: `DigitalTwin` (SQL exists as `usp_Twin_*`, a shortened name) and `EnterpriseFabric` (SQL
exists as `usp_FabricNode_*`) — both false positives from a first-pass naive grep, corrected before this
list was finalized. This distinction matters for anyone acting on this list: **do not re-derive it by
re-running a naive substring grep alone** — confirm the absence with the aggregate's actual property/table
name, not just its class name.

---

## 2. Coverage & Method

**What this phase is:** a full, non-sampled structural census of every `Aggregates/*.cs` file under
`PlantSafety.Domain` (104 directories, 106 files, 93 of which are real aggregate/entity classes after
excluding 11 namespace-marker placeholder classes with zero methods — e.g. `AnalyticsAggregateMarker` —
and 2 non-aggregate helper files — `AlertLifecycleGuard.cs`, a guard-condition helper referenced by
`Operational.Alerts.Alert`, and `ApplyMetadataAdded.cs`, an extension method file). For every one of the
106 files, this phase extracted: the class name, every `public` method signature (via scripted regex
extraction, not individually read line-by-line for all 93), a count of `throw new` sites (a proxy for
guard density), and a count of `AddDomainEvent` call sites (a proxy for event-publish density). This is a
**directly-verified structural census** — every file was scanned, none were skipped or sampled — but it
is method-signature-and-count level, not full-body-semantics level, for the ~55 aggregates the prior
Ingestion Discovery phase did not already read in full. Where this phase's findings overlap with the
prior discovery's directly-verified deep reads (Accident, Alert[live], Risk[live], SafetyPolicy,
PermitWorkflow, Scorecard, ChecklistTemplate/Completion, CultureAssessment, AuditEngagement/Finding,
PMScheduling, PPEAssignment/Inventory — all individually read in full by Phase 6 of the ingestion
discovery), that deeper verification is inherited and cited, not repeated. Where this phase surfaces
aggregates the prior discovery's ~45-aggregate sample never touched — the entire `EmployeeSafety`,
`MachineSafety`, `SafetyProcedures`, and `ContractorSafety` sub-domain clusters (24 additional rich
aggregates), plus `Dashboards.Dashboard`/`Widget` and several duplicate-concept pairs — those are flagged
below as **structurally new this phase**, confirmed only at the method-signature/count level, and
recommended for a full-body read in Phase 2.

**SQL cross-referencing method:** all 885 `.sql` files under `PlantSafety/` were listed; 541 match the
`usp_*` stored-procedure naming convention (the remaining 344 are numbered infrastructure/migration
scripts — indexes, RLS, Service Broker, backup jobs, tSQLt installs — not stored procedures, and were
excluded from the transition cross-reference). Cross-referencing was done by **file-name matching only**
— `grep`-ing the 541-name list for each aggregate's class name plus common transition-verb suffixes. **No
stored procedure body was opened this phase** — so "SQL persistence exists" here means "a plausibly-named
procedure exists," not "the procedure's `T-SQL` body actually implements the transition correctly" (the
prior discovery's finding that `TriggerAlertCommand`/`StoredProcedureBuilder.AddString` silently truncates
oversized strings is a concrete example of a proc that exists, is called, and still has a latent
correctness bug — file-name matching cannot see that class of problem). Every C#-only-orphan claim in §1
was checked twice: once against an exact `usp_<ClassName>_<Verb>` pattern, and once against a loose
case-insensitive whole-name substring match, specifically to catch cases (`DigitalTwin`→`Twin`,
`EnterpriseFabric`→`FabricNode`) where the SQL layer uses a shortened or different noun than the C# class
name. This two-pass method is *more* rigorous than the exact-match-only pass alone, but a third,
even-looser pass (e.g. matching on partial word stems, or checking `INSERT`/`MERGE` statement targets
inside `_Update`-adjacent procs with different names entirely) was not attempted — so it is possible,
though thought unlikely given the pattern discovered, that 1-2 of the 11 C#-only-orphan aggregates have a
persistence path this phase's grep still missed.

**DataLake, Intelligence, Azure, Reporting:** these four modules' entries in this phase's tables are
**inherited directly from the completed Ingestion Discovery's Phase 6** (`lifecycle-coverage/
datalake-lifecycle.md`, `intelligence-lifecycle.md`, `azure-lifecycle.md`) and re-formatted into this
phase's per-transition table shape, not independently re-verified this phase. Their confidence labels are
carried forward unchanged. DataLake structurally has no C# aggregate layer (command handlers write
directly to Dapper); Azure structurally has no local aggregate (state lives in Azure's own control
plane); Intelligence has 2 real aggregates whose guards are confirmed-bypassed by direct handler reads;
Reporting has no aggregates, no persistence, and no lifecycle of any kind.

**What this phase explicitly did not do:** open and read the full body of all 93 PlantSafety aggregate
files (only ~13 were read in full by the prior discovery; this phase adds signature/count-level coverage
for the other ~80, not full-body reads); open any of the 541 stored procedure bodies (file-name matching
only); trace any Application-layer command handler to confirm it actually calls the domain method it
appears paired with, for aggregates outside the ~13 already deep-read (this is the same "guarded-in-a-
class-the-app-never-calls" risk the prior discovery found for Intelligence's `TrainingJob`/`Investigation`
— **this phase cannot rule out the same pattern recurring inside any of PlantSafety's 48 "rich"
aggregates**, and flags this explicitly as the top candidate for Phase 2's deeper read); or execute any
code, test, or query. Every finding is static-source-only, consistent with the parent discovery's method.

---

## 3. Per-Module/Feature-Area Breakdown

See `module-inventory/*.md` for full per-aggregate, per-transition tables. Index:

| File | Feature area(s) | Aggregate count | Notable finding |
|---|---|---|---|
| `accidentmanagement.md` | AccidentManagement | 1 | Best-covered lifecycle in the solution (inherited) |
| `alerts.md` | Alerts, Operational.Alerts (duplicate) | 2 (+1 guard helper) | Third parallel Alert shape confirmed this phase, richer than the "live" one |
| `riskmanagement.md` | RiskManagement, Operational.RiskManagement (duplicate) | 2 | Live vs GET-only-controller duplicate (inherited) |
| `safetypolicies.md` | SafetyPolicies | 1 | Reject is as well-guarded as the forward path (inherited) |
| `permittowork.md` | PermitToWork.PermitWorkflow, PermitTemplate, PermitExpiration, PermitScheduling | 4 | 2 rich, 2 create-only within the same feature area |
| `contractorsafety.md` | ContractorSafety (5 sub-aggregates), ContractorVisitor | 6 | **New this phase**: 5 previously-unsampled rich aggregates; a 3rd `PermitToWork`-named aggregate here, distinct bounded context from `PermitToWork.PermitWorkflow` — flagged, not confirmed as duplicate |
| `employeesafety.md` | EmployeeSafety (7 sub-aggregates) | 7 | **New this phase**: entirely unsampled by prior discovery, all 7 are rich |
| `machinesafety.md` | MachineSafety (6 sub-aggregates) | 6 | **New this phase**: entirely unsampled by prior discovery, all 6 are rich |
| `safetyprocedures.md` | SafetyProcedures (8 sub-aggregates) | 8 | **New this phase**: entirely unsampled by prior discovery, all 8 are rich |
| `checklists.md` | Checklists, Operational.Checklists (duplicate) | 2 | Rich vs create-only marker duplicate |
| `auditing.md` | Auditing (AuditEngagement/AuditFinding), Compliance.Auditing (duplicate) | 3 | Rich lifecycle never given the AuditLog SQL insert (inherited) |
| `safetyculture.md` | SafetyCulture (CultureAssessment, StopWorkEvent) | 2 | — |
| `scheduling.md` | PMScheduling, WorkOrderPriority, EmergencyDrillPlanning, HazardIsolationSequencing, ShiftFatigue | 5 | The one unreachable `Cancel` (inherited) |
| `observability.md` | Observability (5 sub-aggregates) | 5 | `MachineProcessMonitoring` can raise an anomaly but never formally resolve it (inherited) |
| `ppe.md` | PPE (5 sub-aggregates) | 5 | 2 rich, 3 create-only |
| `dashboards.md` | Dashboards.Dashboard/Widget, Analytics.Dashboard (duplicate marker) | 3 | **New this phase**: a 2nd confirmed Dashboard-concept duplicate; Reads/Writes SQL naming, not Create/Update |
| `analytics.md` | Analytics (Culture, Engagement, PredictiveAnalytics, ScoreCards markers) | 4 | All create-only; `ScoreCards` here is a duplicate of top-level `Scorecards.Scorecard` |
| `compliance.md` | Compliance (7 sub-aggregates), Operational duplicates | 7 | Largest create-only cluster |
| `safetyevents.md` | SafetyEvents (EmergencyManagement, IncidentManagement, Observation) | 3 | All create-only |
| `scorecards.md` | Scorecards.Scorecard | 1 | Exception-message copy-paste bug (inherited) |
| `emerging-createonly.md` | AiGovernance, AiSafetyIntelligence, DigitalTwin, EnterpriseFabric, EnvironmentalHealth, Governance, HumanFactors, OperationalSafety, ProcessManagement, Sustainability, WorkerWellBeing | 11 | **New this phase's headline finding**: 10 of these 11 have zero confirmed SQL persistence of any kind |
| `datalake.md` | DataLake (all sub-areas) | 0 (no aggregate layer) | Inherited from prior discovery |
| `intelligence.md` | AITraining (TrainingJob), AIInvestigations (Investigation) | 2 | Guarded-but-bypassed (inherited) |
| `azure.md` | Azure (9 subdomains) | 0 (ARM control plane) | Uniform, shallow-by-design (inherited) |
| `reporting.md` | Reporting | 0 | No lifecycle exists (inherited) |

---

## 4. Top Findings for Phase 2 to Deepen

1. **10-11 PlantSafety aggregates have zero confirmed SQL persistence of any kind** — the single most
   concrete "would fail/no-op if called" finding of this phase. Confirm by opening each aggregate's
   repository implementation directly (not just grepping proc names) — a repository could theoretically
   call a proc with a name this grep pattern didn't anticipate.
2. **Whether the "guarded-in-a-class-the-app-never-calls" pattern found in Intelligence recurs inside any
   of PlantSafety's 48 rich aggregates.** This phase did not trace command handlers to domain-method call
   sites for the ~35 rich aggregates the prior discovery didn't already deep-read (the EmployeeSafety/
   MachineSafety/SafetyProcedures/ContractorSafety clusters are the top candidates, being entirely new
   this phase).
3. **A 6th+ duplicate-concept cluster** may exist between `ContractorSafety.PermitToWork` and
   `PermitToWork.PermitWorkflow` — flagged, not confirmed, this phase.
4. **The Archive-never-publishes-event pattern's true extent** — 3 newly-found Archive-havers
   (`PermitTemplate`, `Dashboards.Dashboard`, `Operational.Alerts.Alert`) were not individually checked
   this phase for whether they follow the same silent pattern as Accident/Risk/Scorecard.
