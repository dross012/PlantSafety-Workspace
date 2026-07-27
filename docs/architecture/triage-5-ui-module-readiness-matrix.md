# Triage 5 — UI Module Readiness Matrix

## Method and honesty constraints

`PlantSafety.UI/Modules/` contains 74 module folders. This session
directly investigated the backend for a small number of them
(`IncidentManagement`, `ContinuousImprovement`/CAPA, `Analytics.Culture`,
`Compliance`, `Scheduling`, `ProcessManagement`, `RiskManagement`,
`AccidentManagement`). For the rest, this matrix uses the **module-family
signals already gathered** in Triage 3 (integration test coverage) and
Triage 4 (SQL error-handling adoption) as a proxy, cross-referenced by
best-effort name matching to a backend `PlantSafety.Infrastructure`
folder. Name matching is not always 1:1 (e.g. several UI "Analytics"
modules map to sub-folders within the single backend `Analytics` module).

**Confidence column meaning**:
- **Direct** — this session directly investigated this specific backend
  module's actual state (not inferred from family-level stats).
- **Inferred** — backend module identified with reasonable confidence by
  name/folder correspondence; readiness based on that module's aggregate
  TRY/CATCH and test-coverage stats from Triage 3/4, not individually
  verified.
- **Unmapped** — no confident backend module correspondence found; no
  readiness claim is made.

Do not treat "Inferred" rows as equivalent in reliability to "Direct"
rows — they are educated estimates from proxy data, not independent
findings.

## Matrix

| UI Module | Backend module(s) | Signal | Readiness | Confidence |
|---|---|---|---|---|
| IncidentManagement | `SafetyEvents.Incident` | Aggregate migrated (Step D), RowVersion bug fixed, 55% TRY/CATCH module-wide | **Ready** (pending Step C test-host proof) | Direct |
| Rca / Observations / NearMiss | `SafetyEvents` (RootCauseAnalysis, Observation, NearMisses — confirmed IntegrationTests folder names) | Same module family as Incident (55% TRY/CATCH); not individually verified | Likely ready, unverified in detail | Inferred |
| CrisisManagement / EmergencyManagement / Drills | `SafetyEvents` (CrisisManagement, EmergencyManagement, DrillsAndExercises) | Same module family (55% TRY/CATCH) | Likely ready, unverified in detail | Inferred |
| Culture / CultureAnalytics | `Analytics.Culture` + `Analytics` (EngagementScoring, CultureIndex, TrendModeling) | **Zero working stored procedures for Culture itself** (Triage 1); `Analytics` module-wide is 0% TRY/CATCH | **Not ready — hard blocker** for Culture write flows | Direct (Culture) / Inferred (Analytics siblings) |
| Compliance / MOC / Permits / RegulatoryCalendar / Certifications / Policies | `Compliance` (ManagementOfChange, Permits, RegulatoryCalendar, Certification, Policy) | Anemic write path (3/124 actions write, Triage-blueprint), 1% TRY/CATCH, **not covered by any integration test schema** | **Not ready for write-flow UI**; read-only UI lower risk | Direct (write-path anemia + error-handling %) |
| Scheduling / SchedulingAnalytics | `Scheduling` | DI registration reportedly incomplete (extraction blueprint), 42% TRY/CATCH | Needs registration-gap confirmation before write-flow UI trusted | Direct (DI gap) / Inferred (error-handling %) |
| ProcessManagement | `ProcessManagement` | RowVersion bug fixed (Triage 2/Step B), only 33% TRY/CATCH, **no integration test schema** | Correctness fix landed but largely unverified end-to-end | Direct |
| PredictiveMaintenance / Maintenance | *(no matching backend module found — see Step G / Triage-blueprint: "Maintenance" doesn't exist as a real bounded context)* | Confirmed absent as a real backend concept | **Not ready — no real backend to build against** | Direct |
| PPE | `PPE` | 42% TRY/CATCH, native `ConcurrencyToken`/`UNIQUEIDENTIFIER` pattern (Triage 2, no known defect), no integration test schema | Plausibly sound, unverified by live test | Inferred |
| Permits (see also Compliance row) | `Compliance.Permits` | Covered under Compliance's anemic-write finding | Same as Compliance | Direct |
| Auditing | `Auditing` | 25% TRY/CATCH, no integration test schema | Elevated risk, unverified | Inferred |
| EnvironmentalHealth | `EnvironmentalHealth` | 0% TRY/CATCH (2 procedures — small module) | Elevated risk, unverified | Inferred |
| HumanFactors / Ergonomics / FatigueManagement | `HumanFactors` / `Operational` subtree | `Operational` module-wide is 3% TRY/CATCH — one of the worst in the codebase | Elevated risk | Inferred |
| HazMat / IndustrialHygiene / MachineManagement / EquipmentSafety / Inspections / Jsa / Loto | `Operational` subtree (best-guess — several `Operational.*` leaf features exist per Step F/Triage 4, individual mapping not confirmed) | Same 3% TRY/CATCH family signal | Elevated risk, low mapping confidence | Unmapped/Inferred (weak) |
| WorkerManagement / WorkerWellBeing | `EmployeeSafety` / `WorkerWellBeing` | 40% TRY/CATCH (EmployeeSafety), unconfirmed for WorkerWellBeing (only 2 procedures found, 0%) | Mixed — WorkerWellBeing specifically elevated risk | Inferred |
| ContractorManagement | `ContractorSafety` / `ContractorVisitor` | 40% TRY/CATCH (ContractorSafety) | Moderate, unverified | Inferred |
| SafetyPrograms / Policies | `SafetyPolicies` | 37% TRY/CATCH | Moderate, unverified | Inferred |
| Checklist | `Checklists` | 30% TRY/CATCH | Elevated risk, unverified | Inferred |
| Training | `Training` | Confirmed: the `Training` backend folder exists but contains **zero** `usp_*.sql` files — no stored procedures at all, same class of gap as Culture | **Not ready if any write flow exists** — verify whether the UI module needs persistence or is display-only | Direct |
| Financials / ROI / ProjectBudgeting | `Financials` | 40% TRY/CATCH (small module, 5 procedures) | Moderate, unverified | Inferred |
| PlanningProjects / ProjectScheduling | `PlanningProjects` | 0% TRY/CATCH (2 procedures) | Elevated risk, unverified | Inferred |
| DigitalTwin | `DigitalTwin` | 33% TRY/CATCH (3 procedures) | Moderate, unverified | Inferred |
| EnterpriseFabric | `EnterpriseFabric` | 33% TRY/CATCH (3 procedures) | Moderate, unverified | Inferred |
| Sustainability | `Sustainability` | 33% TRY/CATCH (3 procedures) | Moderate, unverified | Inferred |
| Governance | `Governance` | Confirmed: the `Governance` backend folder exists but contains **zero** `usp_*.sql` files — same class of gap as `Training` and `Culture` | **Not ready if any write flow exists** — verify whether the UI module needs persistence or is display-only | Direct |
| AiGovernance | `AiGovernance` | 33% TRY/CATCH (3 procedures) | Moderate, unverified | Inferred |
| AiInvestigation / AiTraining | `AiSafetyIntelligence` / `Intelligence` | Neither appeared individually in Triage 4's per-module scan — likely grouped differently or very small procedure counts | **Unassessed** | Unmapped |
| DataLake / PredictiveAnalytics / PredictiveSafety | `Intelligence` (0% TRY/CATCH, 4 procedures, per Triage 4) | Elevated risk | Inferred |
| DashboardAnalytics / EngagementAnalytics / ReportingAnalytics / TrendAnalysis / Scorecards | `Analytics`/`Dashboards`/`Scorecards` (0% and 50% respectively — mixed) | Mixed risk depending on exact sub-area | Inferred (weak — spans multiple backend folders) |
| Alerts | `Alerts` | 75% TRY/CATCH — one of the best-covered modules | Comparatively lower risk | Inferred |
| Exports | `Features` (`ExportContinuousImprovementRepository` confirmed to exist) or `Integrations`/`ExternalServices` | Ambiguous — likely spans several export features per module, not one backend folder | Unmapped |
| DocumentManagement | No confident match found (`FileStorage`? not individually profiled) | — | Unmapped |
| DiagnosticsViewer | Likely the separate `Diagnostics` **repository**, not `PlantSafety.Infrastructure` at all | Out of this session's scope entirely — different repo | Unmapped (wrong repo) |
| Reporting | The separate `Reporting.Api` **repository** (Step E covers its auth config specifically) | Auth startup guard shipped (Step E); its own SQL/error-handling profile was not assessed by Triage 4 (Triage 4 only covers `PlantSafety.Infrastructure`) | Auth ready; backend correctness unassessed | Direct (auth only) |
| OperationsArchival / OperationsAuditViewer / OperationsBackup / OperationsConfiguration / OperationsMonitoring / OperationsSystemHealth | `Operations` (distinct backend folder from `Operational`) | Not individually profiled in Triage 4 (folder wasn't in the per-module scan output — likely zero or very few `.sql` files, consistent with `Operations` being largely a C#-only observability/ops layer rather than a SQL-heavy domain module) | Unassessed | Unmapped |
| ProductionTracking / Routing / SchemaGuard | No confident backend match found | — | Unmapped |
| Admin | Likely spans `Identity`/`Governance`, cross-cutting | — | Unmapped |
| CultureAnalytics *(duplicate of Culture row above — listed once)* | | | | |

## New finding while building this matrix: Culture is not unique

Building this matrix surfaced two more backend module folders
(`Training`, `Governance`) that exist with zero stored procedures at
all — the same class of complete-persistence-gap Triage 1 found for
`Analytics.Culture`, discovered incidentally while checking why they had
no data in Triage 4's table rather than by a targeted search. **This
means the "isolated incident" framing applied to the DI-registration gap
in Triage 3 does not extend to this different, SQL-layer gap** — a
backend folder existing with C# scaffolding but no actual stored
procedures appears to be a repeating pattern, not unique to Culture.
**Recommendation**: run the same check Triage 1 did for Culture (do this
module's repository classes call procedures that don't exist anywhere in
the SQL tree?) against every backend module folder, not just the three
found so far — this was not done exhaustively as part of this triage
series and should be treated as a real follow-up, not an assumption that
only these three are affected.

## Gating criteria (restated from Step G, sharpened with this session's data)

Before treating any "Inferred" or "Unmapped" row above as either safe or
blocked, apply the same checks Step G specified, now with concrete
sources for each:

1. Does the module have any stored procedures at all, and do they
   compile/apply to a real database? (Triage 1's method for Culture is
   the template — don't assume a `.sql` file existing means it was ever
   successfully applied.)
2. What is the module's TRY/CATCH adoption rate (Triage 4's table) — below
   ~40% should be treated as "verify error visibility before shipping a
   UI flow that depends on this module's writes succeeding reliably."
3. Does the module have embedded integration-test schema (Triage 3) —
   if not, its correctness is entirely unproven by this codebase's own
   test suite, regardless of how the C# compiles.
4. Is the module's domain-layer write path anemic (confirmed so far only
   for pre-migration `IncidentManagement` — now fixed — and `Compliance`
   — not yet fixed)? This requires the same kind of direct handler
   inspection Step D/Triage 1 did; it cannot be inferred from SQL stats
   alone.

## What this matrix does not do

It does not claim comprehensive, individually-verified coverage of all 74
UI modules — roughly a third of the rows above are marked "Unmapped" or
rely on weak/ambiguous name-based inference, and that is stated plainly
rather than papered over with false precision. The rows marked "Direct"
(`IncidentManagement`, `Culture`/`CultureAnalytics`, `Compliance`-family,
`Scheduling`, `ProcessManagement`, `Reporting`'s auth) are this session's
actual, verified findings and should be weighted accordingly higher than
any "Inferred" row.
