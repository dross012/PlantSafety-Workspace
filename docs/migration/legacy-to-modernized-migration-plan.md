# PlantSafety Legacy → Modernized Migration Plan

**Status:** Planning document only. No migration has been executed. Nothing in this
document authorizes touching production data or deleting legacy schemas.
**Scope:** Migrating the legacy generic-taxonomy modules (`Compliance`, `Operational`,
`Analytics`, `SafetyEvents`, `Governance`, `OperationalSafety`, plus the `Integrations`/
`CrossDomain`/`Intelligence`/etc. tree) into the 17 domain-pure modules built during the
2026-07-21 modernization pass (`EmployeeSafety`, `ContractorSafety`, `MachineSafety`,
`SafetyProcedures`, `SafetyPolicies`, `AccidentManagement`, `RiskManagement`, `Scorecards`,
`Scheduling`, `PPE`, `PermitToWork`, `Observability`, `Checklists`, `Auditing`,
`SafetyCulture`, `Alerts`, `Dashboards`).
**Database:** `PlantSafety.CoreDB` on `localhost\SQLEXPRESS` (dev). Production topology
(SQLEXPRESS vs. managed Azure SQL, replica count, failover config) is out of scope for this
document — confirm before executing anything below against a non-dev environment.

---

## 0. Precondition — this plan does not start the clock

Before any step in this document can be executed for real, the following must be true.
As of this writing, **they are not**:

| Precondition | Status |
|---|---|
| 17 new modules have real Infrastructure (Dapper + SQL) | ✅ Done |
| RLS protects the 17 new modules' tables | ✅ Done |
| Outbox mechanism actually persists events | ✅ Done (with the `ICommand<T>` gap noted below) |
| All 17 new modules reachable over real HTTP | ✅ Done (322 endpoints) |
| The 151 pre-existing legacy repositories resolve in DI | 🔄 In progress |
| API starts with `ValidateOnBuild = true` (no bypass) | ❌ Blocked on the row above |
| Legacy modules have equivalent lifecycle coverage to their new counterparts | ❌ Not assessed — see §4 |
| A single legacy record has been read through the new pipeline in a non-prod environment | ❌ Not attempted |

Do not schedule a cutover date against this plan until every row above is checked.

---

## 1. Table → Aggregate Mapping

Old tables live in generic per-domain schemas (`Compliance`, `Operational`, `Analytics`,
`SafetyEvents`) where one table often serves several unrelated concepts. New tables are
one-table-per-aggregate (or per-aggregate-plus-child-entity) under module-specific schemas.
This means the mapping is **many-old-columns → few-new-tables**, not 1:1.

| Legacy table | Legacy schema | Target new schema.table(s) | Notes |
|---|---|---|---|
| `Auditing`, `AuditingHistoryEntry` | `Compliance` | `Auditing.AuditEngagement`, `Auditing.AuditFinding` | Legacy table is a flat catch-all; new schema splits engagement vs. finding as separate aggregates with a real 1→N relationship. |
| `Certification`, `CertificationHistoryEntry` | `Compliance` | `SafetyPolicies.SafetyPolicy` (training/cert category), `EmployeeSafety.HazardCommunicationTraining` | Legacy certification records split by whether they represent a policy artifact or an employee training record — requires row-level classification during migration, not a blind copy. |
| `Policy`, `PolicyHistoryEntry` | `Compliance` | `SafetyPolicies.SafetyPolicy`, `SafetyPolicies.PolicyAcknowledgment` | Closest 1:1 mapping in this set — `Policy` → `SafetyPolicy`, any ack/sign-off sub-rows → `PolicyAcknowledgment`. |
| `Procedures`, `ProceduresHistoryEntry` | `Compliance` | `SafetyProcedures.*` (8 sub-aggregates: `HazardCommunicationRightToKnow`, `LotoProcedures`, `MachineGuardingInspections`, `PpePolicies`, `MaterialHandlingStorage`, `ErgonomicsWorkstationDesign`, `EmergencyResponsePlanning`, `RoutineSafetyAuditsIncidentReporting`) | Legacy `Procedures` is undifferentiated; migration requires a `ProcedureCategory`/similar discriminator column to route each row to the correct new sub-aggregate. If no such column exists, this is a manual classification pass, not an automated migration. |
| `Permits`, `PermitsHistoryEntry` | `Compliance` | `PermitToWork.PermitWorkflow`, `PermitToWork.PermitTemplate` | Legacy permits table almost certainly conflates template (reusable permit type) with workflow (a specific instance) — verify via a `PermitType` vs. `PermitId` cardinality check before mapping. |
| `ManagementOfChange`, `...HistoryEntry` | `Compliance` | `RiskManagement.Risk`, `RiskManagement.ControlMeasure` | MOC records are risk assessments in substance; `ControlMeasure` maps to whatever remediation/action columns exist on the legacy row. |
| `RegulatoryCalendar`, `...HistoryEntry` | `Compliance` | `SafetyPolicies.SafetyPolicy` (`ReviewCadence`), `Scheduling.PMScheduling` | Split by whether the row represents a policy review deadline or an operational scheduling event. |
| `Compliance`, `ComplianceHistoryEntry` | `Compliance` | `SafetyPolicies.SafetyPolicy` | Generic catch-all; likely the thinnest/most redundant legacy table — audit its actual row count and column usage before assuming it needs a dedicated target. |
| `Alert`, `AlertHistoryEntry`, `AlertSummary` | `Operational` | `Alerts.Alert`, `Alerts.AlertDeliveryRecord`, `Alerts.AlertResponseAction` | `AlertSummary` likely a denormalized read-model — do not migrate it as data; regenerate it from the new `Alerts.Alert` table via a view or the existing Dashboards widget pipeline instead. |
| `Checklists`, `ChecklistsHistoryEntry` | `Operational` | `Checklists.ChecklistTemplate`, `Checklists.ChecklistCompletion` | Legacy row likely conflates template and completion the same way `Permits` does — verify cardinality first. |
| `Contractor`, `ContractorHistoryEntry` | `Operational` | `ContractorSafety.PreQualificationVetting`, `ContractorSafety.SiteOrientation`, `ContractorSafety.ActiveMonitoringAuditing`, `ContractorSafety.IncidentTrackingPerformanceEvaluation` | One legacy table maps to 4 new sub-aggregates — needs a lifecycle-stage discriminator column to route correctly. |
| `PPEManagement`, `...HistoryEntry` | `Operational` | `PPE.PPEInventory`, `PPE.PPEAssignment`, `EmployeeSafety.Ppe` | Three plausible targets — PPE stock records go to `PPEInventory`, per-employee issuance to `PPEAssignment`/`EmployeeSafety.Ppe`. Requires row classification. |
| `RiskManagement`, `...HistoryEntry`, `RiskManagementSummary` | `Operational` | `RiskManagement.Risk` (schema `[RiskMgmt]`) | Direct candidate for closest-to-1:1 mapping of this whole plan, since both are already risk-shaped. Still requires column-by-column reconciliation (legacy `RiskLevel` values vs. new `RiskLevel` Smart Enum domain). |
| `Schedules`, `SchedulesHistoryEntry` | `Operational` | `Scheduling.PMScheduling`, `Scheduling.WorkOrderPriority` | |
| `Training`, `TrainingHistoryEntry` | `Operational` | `EmployeeSafety.HazardCommunicationTraining`, `SafetyPolicies.SafetyPolicy` (training-category policies) | |
| `Culture`, `CultureHistoryEntry` | `Analytics` | `SafetyCulture.CultureAssessment` | |
| `Dashboard`, `DashboardHistoryEntry` | `Analytics` | `Dashboards.Dashboard`, `Dashboards.Widget` | See §5. |
| `Engagement`, `EngagementHistoryEntry` | `Analytics` | `SafetyCulture.CultureAssessment` (engagement-scoring category) | |
| `PredictiveAnalytics`, `...HistoryEntry` | `Analytics` | `Observability.MachineProcessMonitoring`, `Observability.AutomatedIncidentDetection` | |
| `ScoreCards`, `ScoreCardsHistoryEntry` | `Analytics` | `Scorecards.Scorecard`, `Scorecards.KpiMeasurement` | |
| `EmergencyManagement`, `...HistoryEntry` | `SafetyEvents` | `EmployeeSafety.EmergencyPreparedness`, `AccidentManagement.Accident` (severity-triggered rows) | |
| `IncidentManagement`, `...HistoryEntry`, `IncidentManagementSummary` | `SafetyEvents` | `AccidentManagement.Accident`, `AccidentManagement.CorrectiveAction` | Second-closest-to-1:1 candidate after `RiskManagement`. |
| `Observation`, `ObservationHistoryEntry` | `SafetyEvents` | `Observability.PPEBehavioralMonitoring`, `SafetyCulture.CultureAssessment` | |
| `Governance` | `Governance` | Not mapped — no clear 1:1 new-module owner. Flag for a scoping decision before migration: does this become part of `SafetyPolicies`, or does it stay a standalone concern? |
| `OperationalSafety` | `OperationalSafety` | Not mapped — appears to be a single umbrella table; needs its own discovery pass. |

**Action item before executing anything in this section:** every "requires row classification"
note above means someone must run a real `SELECT DISTINCT` / cardinality audit against the
live legacy tables to confirm the actual column shapes before writing migration scripts —
this plan documents *where things should go*, not a verified column-level ETL spec.

---

## 2. Stored Procedure → Lifecycle Mapping

Legacy procedures are largely flat CRUD (`usp_<Table>_Get`, `usp_<Table>_Insert`,
`usp_<Table>_Update`, plus a handful of `_BulkUpdate` procs per module found during this
session's DI-resolution audit). New modules expose one procedure per real domain lifecycle
transition (e.g. `usp_PermitWorkflow_AssessHazards`, `usp_Accident_AssignInvestigator`).

| Legacy pattern | New pattern | Migration implication |
|---|---|---|
| `usp_<Table>_Insert` (single flat insert covering every field) | `usp_<Aggregate>_Insert` (create) + N `usp_<Aggregate>_<Transition>` procs | A legacy insert that set a "final" status directly (e.g. inserting a `Closed` accident) has no equivalent single call in the new schema — it must be replayed as a **sequence** of lifecycle calls (Create → AssignInvestigator → RecordRootCause → ... → Close) to end up in a valid new-schema state, or the migration script must bulk-write final state directly into the new tables (bypassing the command layer) and accept that in-flight domain events for intermediate transitions are not retroactively created. **Decision needed**: replay history through commands (slow, produces a full audit trail) vs. direct-write final state (fast, no synthetic history). Recommend direct-write for bulk historical backfill, command-replay only for a small pilot batch used to validate the mapping. |
| `usp_<Table>_Update` (single flat update, any field) | Per-transition procs, each only updating the columns relevant to that transition | Same implication as above — a legacy `Update` call that changed 5 unrelated fields at once has no single new-schema equivalent. |
| `usp_<Module>_BulkUpdate<Module>Command` (generic bulk endpoint, found in `Compliance`, `Operational`, `SafetyEvents` during this session's audit) | No equivalent — new modules do not have a generic bulk-update escape hatch | Any legacy caller depending on this generic bulk endpoint needs a real replacement command identified per call site before cutover; there is no drop-in substitute. |
| Legacy read procs return wide, denormalized rows (all columns, all history in one call) | New read procs are narrow and per-query-shape (e.g. `usp_Risk_GetAllActive` vs. a 30-column `usp_RiskManagement_Get`) | Any legacy consumer parsing the wide row shape needs updated column mapping, not just a new proc name. |

**Action item:** produce an actual proc-by-proc call graph (which legacy procs are called
by which legacy handlers, and how often based on production telemetry if available) before
deciding which procs get a scripted replay vs. a manual one-off conversion.

---

## 3. Read Model → Projection Mapping

| Legacy read model | Shape | New projection | Notes |
|---|---|---|---|
| `AlertSummary` | Denormalized, likely pre-aggregated counts | `Alerts.Alert` + a new summary view/widget over it (build via the `Dashboards.Widget` mechanism, not a hand-rolled summary table) | Do not migrate `AlertSummary` as data — it's a cache, not a source of truth. Regenerate. |
| `IncidentManagementSummary` | Same pattern | `AccidentManagement.Accident` + a `Scorecards`/`Dashboards` projection | Same recommendation — regenerate, don't migrate. |
| `RiskManagementSummary` | Same pattern | `RiskManagement.Risk` + projection | Same recommendation. |
| Per-category "analytics" query handlers built for the 17 new modules this session (e.g. `Checklists` 8 category queries, `SafetyPolicies` 17 category queries) | Already exist, already live | N/A — these are the *replacement* projections; legacy summary tables should ultimately be retired in favor of these once populated with migrated data. |

General rule for this section: **legacy pre-aggregated/summary tables are not migration
targets — they're the exact kind of denormalized cache the new CQRS read-model pattern
replaces.** Only migrate normalized source-of-truth tables (§1); let summary/projection
tables regenerate from the new aggregates once real data exists there.

---

## 4. Workflow → Command Mapping

This is the section most exposed by this session's own findings, so it's worth stating
plainly: **as of this writing, some of the 17 new modules do not yet have full lifecycle
command coverage**, discovered via a real end-to-end round-trip test:

- `PermitToWork.PermitWorkflow` was missing `AssessHazards`/`ImplementControls` commands
  until a targeted fix earlier this session (now closed, verified via live DB round-trip).
- No other module has been proven end-to-end the same way — the PermitToWork gap was found
  because it was one of five modules put through a real multi-step lifecycle test. The other
  12 modules have not had the same scrutiny.

**Before mapping any specific legacy workflow to a specific new command sequence**, each of
the 17 new modules' full lifecycle needs the same treatment PermitToWork got: walk the real
state machine (`Requested → ... → Closed`, or equivalent) against the live database and
confirm every transition has a real, reachable HTTP command. Do this as an explicit
pre-migration task, not discovered mid-migration.

Once that's done, the actual mapping is straightforward in form (legacy multi-purpose
`_Update` call → new `<Transition>Command`), and the real content of this section is:
*produce a per-module table of "legacy action X → new command Y" once each new module's
command surface is confirmed complete.* That table cannot be written correctly today because
the underlying command surface is only confirmed complete for 5 of 17 modules.

---

## 5. Dashboard → Widget Mapping

The legacy `Analytics.Dashboard`/`DashboardHistoryEntry` tables and the new
`Dashboards.Dashboard`/`Dashboards.Widget` tables are the two closest-to-compatible schemas
in this whole plan (same session built the new one with the legacy widget catalog as a
source — see the `WidgetType` Smart Enum, which already enumerates all 10 Appendix E widget
types found in the legacy design docs). Recommended mapping approach:

1. Each legacy `Dashboard` row → one new `Dashboards.Dashboard` row, mapped via the existing
   `DashboardCategory` Smart Enum (19 values already enumerated against the legacy program's
   full dashboard list — confirmed during this session's Alerts/Dashboards deepening pass).
2. Each legacy dashboard's constituent chart/panel definitions → individual
   `Dashboards.Widget` rows, mapped via the `WidgetType` enum.
3. Legacy `DashboardHistoryEntry` (audit trail) → do not migrate as data; it's redundant with
   the new outbox/domain-event trail once the outbox is fully wired (see the platform-wide
   `ICommand<T>` gap noted in the precondition table above).

This is the one section of this plan that could plausibly be a **fully automated** migration
script rather than a manual classification pass, precisely because the new schema was
designed with the legacy widget catalog as an input.

---

## 6. Alert → Escalation Chain Mapping

Legacy `Operational.Alert` has a flat `AlertHistoryEntry` audit trail with no explicit
escalation state machine visible in the schema inventory gathered for this plan. The new
`Alerts.Alert` aggregate has a real state machine
(`Triggered → Acknowledged → Dispatched → Escalated → Resolved → Closed`) with a real
`AlertEscalationPolicy` (time-threshold-driven auto-escalation, confirmed built and verified
this session).

**Mapping approach:**
1. Legacy alerts with a terminal state (resolved/closed) → direct-write final state into
   `Alerts.Alert` (per the §2 "direct-write vs. replay" decision — recommend direct-write
   here specifically, since alert escalation timing is unlikely to be reconstructible from
   legacy data with confidence).
2. Legacy alerts in a non-terminal state at migration time → these need a manual review pass,
   not an automated migration, since the new escalation policy will start evaluating
   auto-escalation thresholds against them immediately based on `TriggeredAtUtc` — a
   long-open legacy alert could trigger a flood of auto-escalations the moment it's migrated
   if not handled carefully (e.g. backdate `TriggeredAtUtc` accurately, or explicitly migrate
   into `Acknowledged` state to suppress the auto-escalation timer).
3. The legacy `AlertSummary` table (see §3) is not migrated — it's a cache.

---

## 7. Safety Event → Ingestion Pipeline Mapping

"Safety event ingestion pipelines" (the mechanism by which incidents/near-misses/observations
enter the system, e.g. from IoT sensors, mobile app submissions, or manual entry) are **not
part of the 17-module modernization scope** — none of the 17 new modules replace an ingestion
pipeline; they replace the domain/persistence layer that an ingestion pipeline would write
into. The legacy `SafetyEvents.*` tables (`IncidentManagement`, `EmergencyManagement`,
`Observation`) map to new aggregates per §1, but whatever currently populates those legacy
tables (webhook handlers, IoT event consumers, mobile API endpoints) needs its own,
separate discovery: **audit every current write path into the legacy `SafetyEvents` schema
before assuming this migration plan covers them.** This session did not build or audit any
ingestion pipeline — flagging this as a real scope gap in the plan, not glossing over it.

---

## 8. Migration Script Design

Recommended structure, informed by what actually exists today:

1. **Per-table migration script** (one `.sql` file per legacy table, e.g.
   `migrate_Compliance_Policy_to_SafetyPolicies.sql`), each performing:
   - A `SELECT` from the legacy table joined against whatever discriminator/classification
     logic §1 determined is needed.
   - An `INSERT` into the new schema's table(s), generating new `StrongId` GUIDs (do not
     reuse legacy integer/GUID keys blindly — check whether the new schema's ID generation
     convention, `Uuid7`/`Guid.NewGuid()` per this session's established pattern, is
     compatible with a bulk-insert of pre-existing legacy IDs, or whether a legacy-ID → new-ID
     mapping table is needed for referential integrity during the transition period).
   - A row-count and checksum verification step (see §11).
2. **Idempotency**: every script must be safely re-runnable — check for an
   already-migrated marker (e.g. a `LegacySourceId` column on the new table, or a separate
   `Migration.MigratedRows` tracking table) before inserting, so a partial/failed run can be
   resumed without duplicating data.
3. **Batching**: migrate in bounded batches (recommend starting at 1,000 rows/batch given no
   current production volume data to size against), not single-transaction full-table moves,
   to keep lock duration bounded on a live system.
4. **Tenant scoping**: every new table has `TenantId NOT NULL` (RLS-enforced). Confirm the
   legacy tables actually carry a reliable `TenantId` column before migration — if legacy data
   predates multi-tenancy or has an unreliable tenant column, this is a blocking data-quality
   issue to resolve before any script can run, not something the migration script can paper
   over.

This document does **not** include the actual `.sql` script bodies — writing 20+ real,
tested migration scripts against unverified legacy column shapes (see §1's action item) would
be guessing at data that hasn't been audited yet. That's the next concrete deliverable once
someone runs the column-shape audit this plan calls for.

---

## 9. Cutover Strategy

Recommended approach: **strangler-fig, per-module, not big-bang.**

1. **Per module, not all 17 at once.** Pick the module with the cleanest 1:1 mapping first —
   per §1, that's `Operational.RiskManagement → RiskManagement` or
   `SafetyEvents.IncidentManagement → AccidentManagement`. Use it as the pilot.
2. **Dual-write phase** (per module): new commands write to the new schema (already true
   today); add a synchronized write-through to the legacy schema for a bounded period so
   legacy read paths (reports, other integrations not yet migrated) keep working during
   transition. This requires a new, explicit piece of code — nothing built this session
   does this.
3. **Read cutover**: once the dual-write phase has run long enough to build confidence
   (recommend: a full business cycle — e.g. one full audit/reporting period — not just a
   fixed number of days), switch read paths to the new schema. Keep legacy writes flowing
   for one more cycle as a safety net.
4. **Write cutover**: stop writing to the legacy schema. Legacy tables become read-only
   archives.
5. **Repeat per module**, in order of mapping cleanliness (cleanest first, most-ambiguous
   last — defer `Compliance.Compliance`/`Governance`/`OperationalSafety`, the three tables
   in §1 with no clear target, until every other module has proven the process).
6. **Do not schedule specific calendar dates in this document** — they depend on real data
   volume and business-cycle timing this plan does not have access to.

---

## 10. Rollback Strategy

1. **During dual-write phase**: rollback is trivial — stop routing reads to the new schema,
   nothing was lost since legacy writes never stopped.
2. **After write cutover, before legacy tables are dropped**: rollback means reversing
   direction — resume legacy writes (requires the dual-write code from §9 step 2 to be kept
   in place, not deleted, until this module's migration is fully retired from the rollback
   window) and replaying any new-schema-only writes back into the legacy schema. This is
   real, nontrivial work per module — budget for it, don't assume "just flip a flag."
3. **Point-of-no-return**: legacy tables should not be dropped until a defined number of full
   business cycles have passed with zero rollback incidents (recommend: at minimum the same
   cycle count used to build confidence in §9 step 3, doubled). Document the actual drop
   as a separate, explicitly-approved action — this plan does not authorize it.
4. **RLS/tenant-isolation rollback risk**: the new schema's RLS policy (`TenantAccessPolicy`)
   is deny-by-default. If a rollback needs to route traffic back to legacy tables that have
   no equivalent RLS protection, confirm the legacy tables' isolation mechanism (application-
   level tenant filtering, presumably, per the RLS migration script's own documented
   discovery this session) is still intact and hasn't atrophied during the time the new
   schema was primary.

---

## 11. Verification Strategy

1. **Pre-migration**: row-count and checksum baseline per legacy table (the column-shape
   audit called for in §1 produces this as a byproduct).
2. **Post-migration, per batch**: row-count reconciliation (migrated new-schema row count
   matches legacy source row count for that batch, accounting for any 1-legacy-row →
   N-new-rows splits documented in §1) plus spot-check field-level comparison on a random
   sample (recommend 1% or 50 rows, whichever is larger, per batch).
3. **Functional verification**: reuse the pattern already proven this session — a real
   command → handler → repository → stored procedure → table round-trip, run against
   migrated data specifically (not just fresh test data), for at least one record per
   migrated module, following the same standard used for the PermitToWork/Alerts/RiskManagement
   smoke tests earlier in this session (direct SQL evidence, not "should work").
4. **RLS verification**: for every migrated table, run the same before/after RLS check used
   to close the RLS gap this session (no session context → 0 rows; correct tenant context →
   real rows; wrong tenant context → 0 rows) against migrated data specifically, not just
   freshly-inserted test rows.
5. **Outbox verification**: once the `ICommand<T>` gap is closed platform-wide, confirm
   migrated-then-mutated records actually produce outbox entries — this is a real behavioral
   difference from legacy data that never went through the outbox mechanism.
6. **Sign-off gate**: no module's write-cutover (§9 step 4) proceeds without all 5 checks
   above passing and being recorded somewhere durable (this document does not specify where
   — that's an organizational decision, not a technical one).

---

## Summary of open items this plan surfaces (do not skip)

1. Column-shape audit of every legacy table in §1 — this plan describes *targets*, not
   verified *transformations*.
2. Decision: replay-through-commands vs. direct-write for historical data (§2).
3. Full lifecycle-command-coverage audit for the 12 new modules not yet proven end-to-end
   the way PermitToWork/Alerts/RiskManagement/Auditing/Dashboards were (§4).
4. Discovery of what currently writes into the legacy `SafetyEvents` schema — no ingestion
   pipeline was built or audited this session (§7).
5. `Governance`, `OperationalSafety`, and the thin `Compliance.Compliance` table have no
   clear new-module target — needs a scoping decision before migration, not during it.
6. The platform-wide `ICommand<T>` / outbox-auto-flush gap (documented separately) should be
   closed before cutover, since §11's outbox verification step depends on it.
