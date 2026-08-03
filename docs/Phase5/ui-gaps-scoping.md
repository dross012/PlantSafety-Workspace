# UI Gaps Scoping — 9 Modules With No Backing Application/API Layer

**Status:** Investigation only. No C#/SQL written. No files modified except this document.
**Source of gap list:** Platform-wide API contract audit, 2026-08-02/03.
**Scope of this audit:** For each module — full `ApiClient` (every route/DTO), the module's `Pages/*.razor`
(what the page actually shows/does), and whether an existing Application-layer module already covers
the same domain under a different name.

## Cross-cutting findings that apply to all 9

- All 9 modules are wired into the main navigation (`PlantSafety.UI/Routing/ModuleRouteRegistry.cs`,
  `"Core Operational"` section, `SectionOrder` 10–18: scheduling, maintenance, predictive-maintenance,
  production-tracking, routing, reporting, safety-programs, incident-management, document-management).
  **None of the 9 are orphaned/unreachable pages** — every one is a first-class nav entry a user can click
  today and get a broken/degraded page. This raises the practical urgency of all 9 roughly equally; there
  is no "hidden feature nobody sees" excuse for any of them.
- Every `ApiClient` file already carries a header comment written by whoever scaffolded the UI ("...is not
  yet backed by a controller...the UI degrades gracefully via `ErrorBoundaryComponent`"). This means the
  original author knew these were UI-first stubs. The Handbook's "Additive-only evolution" note (§XI) is
  cited in every file header, meaning any Application/API build-out for these must follow the platform's
  established CQRS + versioned-controller + repository pattern (see `PlantSafety.Application/Operational/PreventiveMaintenance`,
  `PlantSafety.Application/ProcessManagement`, `PlantSafety.Application/Compliance/Certification` as
  representative "fully built" module shapes with `Commands/`, `Queries/`, `Handlers/`, `Models/`,
  `Repositories/`, `Validation/`, `DependencyInjection/`).
- Route-prefix collisions are a real risk: several of these UI modules choose a route prefix (`v1/scheduling/*`,
  `v1/maintenance/*`, `v1/predictive-maintenance/*`, `v1/reporting/*`, `v1/documents`, `v1/routing/*`) that is
  either already partially used by a *different, narrower* real controller, or immediately adjacent to one
  (e.g. `v1/operational/predictive-maintenance` vs. `v1/predictive-maintenance`). Any build-out must decide
  explicitly whether to relocate the UI to the existing prefix or give the new domain a disambiguated prefix
  — this is a design decision, not just a coding task.

---

## 1. IncidentManagement

**ApiClient:** `PlantSafety.UI/Modules/IncidentManagement/ApiClients/IncidentManagementApiClient.cs`
**Pages:** `IncidentsListPage.razor`, `IncidentDetailPage.razor`, `IncidentTimelinePage.razor`, `IncidentSlaMonitorPage.razor`

**What's real today:** `GetIncidentsAsync` (list, paginated, severity/status filter) and `GetIncidentAsync`
(detail by id) map cleanly onto `PlantSafety.Api/SafetyEvents/IncidentManagement/Controllers/V1/IncidentsCanonicalV1Controller.cs`
(`v1/safety-events/incidents`). That controller is backed by real Application-layer incident read logic under
`PlantSafety.Application/SafetyEvents` (the canonical incident aggregate lives there; note there is a
*separate*, thin `PlantSafety.Application/SafetyEvents/IncidentManagement` folder containing only a single
`GetIncidentManagementQueryHandler` — a legacy/parallel query slice, not the same thing as the canonical
incidents surface backing the controller above; don't confuse the two when building).

**What's missing:** `timeline`, `assign`, `escalate`, `notes`, `resolve` (5 command/query endpoints on an
existing incident), plus `sla-monitor/incidents`, `sla-monitor/risk`, `sla-monitor/trend` (3 endpoints implying
an entirely separate SLA policy/threshold engine). Grepping the whole `PlantSafety.Application` tree for
SLA/threshold/escalation-timer concepts turns up **nothing** — there is no SLA policy engine anywhere in the
platform to extend.

- **Reuse vs. new-build:** Split assessment. The base list/detail is a genuine reuse case (already works).
  The workflow actions (assign/escalate/notes/resolve/timeline) are a **small-to-medium addition** to the
  existing canonical incident aggregate — they're natural incident-lifecycle commands that belong right next
  to whatever already creates/reads incidents. The SLA monitor is a **genuinely new domain** (a policy engine:
  SLA definitions per severity, countdown computation, breach/risk classification, trend aggregation) with no
  analog anywhere in the codebase.
- **Scope size estimate:** Lifecycle actions: ~5 commands (`AssignTechnicianToIncidentCommand`,
  `EscalateIncidentCommand`, `AddIncidentNoteCommand`, `ResolveIncidentCommand`) + 1 query (`GetIncidentTimelineQuery`),
  each with handler/validator, plus 2-3 new domain events (`IncidentAssigned`, `IncidentEscalated`, `IncidentResolved`).
  SLA monitor: 1 new aggregate/policy concept (`IncidentSlaPolicy`) + 3 queries + a scheduled/computed
  breach-risk classifier — comparable in size to a small new bounded context, not just a few CQRS handlers.
- **Suggested approach:** Extend the existing canonical incident aggregate/controller with lifecycle commands
  first (highest value, lowest risk, page is already half-working). Treat `sla-monitor/*` as a distinct,
  explicitly-scoped follow-on epic: define an SLA policy configuration model (per-severity response/resolution
  targets), a breach-risk calculator, and trend rollup — likely reusing the Scorecards module's aggregation
  patterns (`PlantSafety.Application/Scorecards/LeadingIndicators` etc.) as a structural template rather than
  inventing a new aggregation approach from scratch.
- **Dependencies/risks:** SLA thresholds need a source of truth (per-severity targets) — is that configuration
  data, a new admin-editable policy, or hardcoded constants? That's a product decision, not just an engineering
  one, and blocks the SLA sub-scope until answered. The lifecycle commands need to agree on the same
  authorization/ownership model already governing incident creation (who can assign/escalate/resolve) — check
  `PlantSafety.Application/SafetyEvents/Permissions` and `Policies` before designing.

---

## 2. ProductionTracking

**ApiClient:** `PlantSafety.UI/Modules/ProductionTracking/ApiClients/ProductionTrackingApiClient.cs`
**Pages:** `ProductionDashboardPage.razor`, `ProductionRunDetailPage.razor`, `BatchTrackingPage.razor`,
`ProductionTimelinePage.razor`, `KpiDashboardPage.razor`, `EventHistoryPage.razor`

**What's real today:** Nothing. No `v1/production-tracking/*` route exists anywhere in `PlantSafety.Api`.

**What's missing:** Everything — throughput/bottleneck summaries, paginated active-runs list, run detail +
metadata + timeline + batches + KPIs + events (5 sub-resources per run), a cross-run batches list, plant-wide
KPI tiles, plant-wide timeline, and a searchable event-history log. 13 endpoints total across 6 pages — this
is the single largest scope of the 9.

**Overlap check:** `PlantSafety.Application/Observability/Telemetry/ProductionTelemetryStreamProcessor.cs`
is the only file in the entire codebase with "production" + "telemetry" semantics, and it's a single stream
processor (ingestion-side), not a queryable CQRS module with runs/batches/KPI aggregates. There is no
"ProductionRun", "Batch", or production-KPI-engine concept anywhere in Domain/Application. This is **not**
a reskin of an existing module.

- **Reuse vs. new-build:** Genuinely new domain. The only reusable piece is the telemetry stream processor as
  a potential *event source* feeding a new Production Run/Batch aggregate, not as the domain model itself.
- **Scope size estimate:** Large. Needs new aggregates (`ProductionRun`, `Batch`), ~4-6 commands if runs are
  ever mutated from this surface (likely most of this module is read-only reporting off telemetry, so may be
  closer to 10-12 queries + projections than commands), plus a KPI computation engine (throughput, bottleneck
  detection) that doesn't exist. Comparable to or larger than the Workflow module built in Phase 5.
- **Suggested approach:** New maximalist module needed. Given the telemetry stream processor already exists,
  the sane path is: define `ProductionRun`/`Batch` as read-model projections built from the existing telemetry
  event stream (event-sourced/projection style) rather than a fully separate write-side aggregate — this
  avoids duplicating a new source of truth for something that's fundamentally observability data. Coordinate
  with whoever owns `Observability/Telemetry` before designing the read models.
- **Dependencies/risks:** Bottleneck detection and throughput calculation are analytical/algorithmic work, not
  just CRUD — underestimate this at your peril. If telemetry doesn't currently capture run/batch boundaries
  (start/stop/batch-id), this needs new ingestion-side schema changes too, not just a new read side. Highest
  risk-of-hidden-scope of all 9 modules.

---

## 3. Maintenance

**ApiClient:** `PlantSafety.UI/Modules/Maintenance/ApiClients/MaintenanceApiClient.cs`
**Pages:** `MaintenanceDashboardPage.razor`, `WorkOrderDetailPage.razor`, `MachineHistoryPage.razor`,
`PreventiveSchedulePage.razor`, `TechnicianWorkloadPage.razor`, `PartsInventoryPage.razor`, `MaintenanceCalendarPage.razor`

**What's real today:** Nothing at `v1/maintenance/*`. The real, narrower module is
`PlantSafety.Application/Operational/PreventiveMaintenance` + `PredictiveMaintenance`, exposed at
`api/v{version}/operational/preventive-maintenance` (V1Controller + V2Controller — real CQRS, real
repository). Route prefixes are different (`v1/maintenance` vs `v1/operational/preventive-maintenance`) and
shapes don't match: the real controller has no concept of "work order", "parts", "technician workload", or
a maintenance calendar.

**What's missing:** Work-order summary, machine-status summary, paginated work-order list + detail + tasks +
parts + history + predictive-insights sub-resource, machine history, preventive-task list, technician
workload, parts inventory, and a maintenance calendar. 12 endpoints.

- **Reuse vs. new-build:** Mixed. "Preventive schedule" *conceptually* overlaps with the real
  `Operational.PreventiveMaintenance` module and should probably surface that module's data rather than invent
  a second "preventive maintenance" concept — recommend rewiring `GetPreventiveTasksAsync` to read from
  the existing PM schedule aggregate. Everything else (work orders, parts inventory, technician workload,
  calendar, machine history) has no existing analog and is a new build. Note the UI client itself calls a
  `predictive` sub-resource per work order (`v1/maintenance/work-orders/{id}/predictive`) that should
  delegate to the real `Operational.PredictiveMaintenance` module rather than duplicating prediction logic —
  another reuse point.
- **Scope size estimate:** Medium-large. New: `WorkOrder` aggregate (create/assign/task-tracking/parts-consumption/close
  lifecycle — realistically 6-8 commands), `PartInventory` read/adjust model, `TechnicianWorkload` aggregation
  query, `MaintenanceCalendarEvent` projection. Reused: preventive task list (delegate to existing PM module),
  predictive insights (delegate to existing Predictive Maintenance module).
- **Suggested approach:** New maximalist "Work Order Management" module for the core (work orders/parts/technicians/calendar),
  explicitly designed to call into `Operational.PreventiveMaintenance` and `Operational.PredictiveMaintenance`
  for the two sub-resources that already have real logic elsewhere, rather than re-implementing preventive
  scheduling or predictive scoring a second time. This is the clearest "some reuse, mostly new build" case of
  the 9.
- **Dependencies/risks:** Naming collision risk is high — "Maintenance" the UI module vs. "PreventiveMaintenance"/
  "PredictiveMaintenance" the real Application modules will confuse future contributors unless the relationship
  (Work Orders is the operational execution layer; Preventive/Predictive are the planning/scoring layers that
  feed it) is documented explicitly at build time.

---

## 4. PredictiveMaintenance

**ApiClient:** `PlantSafety.UI/Modules/PredictiveMaintenance/ApiClients/PredictiveMaintenanceApiClient.cs`
**Pages:** `PredictiveDashboardPage.razor`, `EquipmentPredictiveViewPage.razor`, `PredictionsListPage.razor`,
`PredictionDetailPage.razor`, `RecommendationDetailPage.razor`, `SensorExplorerPage.razor`, `PredictiveReportsPage.razor`

**What's real today:** `PlantSafety.Api/Operational/PredictiveMaintenance` has `PredictiveMaintenanceV1Controller`,
`V2Controller`, and `PredictiveMaintenanceAlertV1Controller` at `api/v{version}/operational/predictive-maintenance[/alerts]`
— real CQRS behind it (`PlantSafety.Application/Operational/PredictiveMaintenance`). Per the audit, this exposes
exactly one GET; the UI client expects 13 sub-resources under a *different* route prefix (`v1/predictive-maintenance/*`
vs. `v1/operational/predictive-maintenance`).

**What's missing:** risk-summary, equipment-health, predictions list (active + full), equipment predictive view,
prediction summary/explanation/forecast/recommendations, recommendation detail, sensor data + anomalies, and
predictive reports list. 13 endpoints — second-largest scope after ProductionTracking.

- **Reuse vs. new-build:** This is the clearest **"rewire + extend"** case of the 9. The domain concept
  (predictive maintenance) genuinely already exists with a real aggregate and alerting controller; the UI was
  simply built against an imagined route prefix and a much richer feature set (model explainability, sensor
  anomaly detection, forecast curves, recommendations) than what the real module currently does. Do not build
  a second "predictive maintenance" domain — extend `Operational.PredictiveMaintenance`.
  Recommend renaming the UI's route prefix to `v1/operational/predictive-maintenance/*` to match the real
  controller once expanded, rather than adding a second prefix for the same concept.
- **Scope size estimate:** Medium. Existing aggregate covers "prediction" already (confirm exact shape before
  scoping precisely — worth a follow-up read of `PlantSafety.Application/Operational/PredictiveMaintenance/Models`).
  New: equipment-health rollup, model-explanation/feature-importance exposure (implies the prediction model
  already stores explainability metadata — verify), forecast curve endpoint, recommendation sub-aggregate
  (~3-4 commands/queries), sensor data + anomaly detection (this may already partially exist under
  `Observability/PPEBehavioralMonitoring`/`MachineProcessMonitoring`/`EnvironmentalAssetMonitoring` — check for
  a generic "sensor data" capability there before building a new one), and a predictive-reports export list.
- **Suggested approach:** Extend `Operational.PredictiveMaintenance` with new queries reusing its existing
  prediction aggregate; check `Observability/*Monitoring` modules for reusable sensor-data plumbing before
  building a new sensor ingestion/query path; treat model-explanation and forecast-curve as genuinely new
  (ML-adjacent) capability that needs product input on what "explanation" data the model actually produces
  today, if any.
- **Dependencies/risks:** If the underlying prediction model doesn't currently persist feature-importance/
  explanation data, `GetModelExplanationAsync` can't be honestly implemented without a model/pipeline change,
  which is outside pure Application-layer scope. Flag this to product before committing to the full 13-endpoint
  scope in one pass.

---

## 5. Scheduling

**ApiClient:** `PlantSafety.UI/Modules/Scheduling/ApiClients/SchedulingApiClient.cs`
**Pages:** `SchedulingDashboardPage.razor`, `JobDetailPage.razor`, `SchedulingCalendarPage.razor`

**What's real today:** Nothing at `v1/scheduling/jobs*` or `v1/scheduling/calendar`. The real
`PlantSafety.Application/Scheduling` module covers PMScheduling, WorkOrderPriority, EmergencyDrillPlanning,
HazardIsolationSequencing, ShiftFatigue, and PredictiveScheduling — all safety/maintenance-planning concepts,
none of which model a generic "Job" (with timeline, constraints, predictive insights) or a generic calendar.

**What's missing:** Job summary, resource summary, paginated upcoming-jobs list, job detail, job timeline,
job constraints, job predictive-insights, calendar week view. 8 endpoints, smallest surface of the 4 "core
operational scheduling/maintenance" modules.

- **Reuse vs. new-build:** Genuinely new — "Job" as a generic schedulable unit doesn't exist. However this is
  suspiciously close in shape to Maintenance's `WorkOrder` (both have: summary counts, paginated list w/
  status filter, detail, timeline, and a "predictive insights" sub-resource) and to Routing's `Approval`/`Steps`
  patterns. Strongly consider whether "Scheduling Jobs" should actually just be **the scheduling view of Work
  Orders** (Maintenance) plus other schedulable entities (drills, PM tasks) rather than a fourth independent
  "Job" concept — i.e., this might legitimately collapse into a calendar/aggregation layer over
  `Operational.PreventiveMaintenance`, the new Maintenance work-order module (see #3), and
  `Scheduling.EmergencyDrillPlanning`, rather than a standalone domain.
- **Scope size estimate:** Small-medium if built as an aggregation/calendar layer over existing scheduling
  sub-modules (mostly new queries, few/no new commands); medium-large if built as an independent "Job" aggregate.
- **Suggested approach:** Before writing a single command, resolve the "what is a Job" question with product —
  likely resolve as: a read-model/calendar aggregation over existing `Scheduling.*` sub-modules and the new
  Maintenance work-order module, exposed as generic "jobs" for the dashboard/calendar UI. This keeps scope
  small and avoids yet another duplicate scheduling concept.
- **Dependencies/risks:** Depends on Maintenance's work-order build (#3) landing first if Jobs are meant to
  surface work orders; sequencing matters — do not build Scheduling's Job aggregate independently and then
  have to reconcile two "job/work order" identities later.

---

## 6. DocumentManagement

**ApiClient:** `PlantSafety.UI/Modules/DocumentManagement/ApiClients/DocumentManagementApiClient.cs`
**Pages:** `DocumentsListPage.razor`, `DocumentDetailPage.razor`

**What's real today:** `PlantSafety.Api/Integrations/Documents` (`DocumentsV1Controller`/`V2Controller` at
`v1/integrations/documents`) — a real, fully-structured module (Authorization, Contracts, Diagnostics,
Endpoints, Filters, Mapping, Models, Telemetry — this is one of the more mature "Integrations" sub-modules)
but exposes one unfiltered GET, no by-id/detail route, no category/search filtering, no pagination in the
shape the UI expects.

**What's missing:** Paginated + category-filtered + search-filtered list, and a detail-by-id endpoint. Only
2 endpoints — smallest scope of the 9.

- **Reuse vs. new-build:** **Rewire/extend**, not new-build. This is the cleanest case in the whole set: a
  real, well-structured module already exists one route-segment away (`v1/integrations/documents` vs.
  `v1/documents`). The fix is almost certainly (a) point the UI at `v1/integrations/documents` (rename the
  route prefix in the ApiClient) and (b) add pagination/filter/search query parameters and a by-id GET to the
  existing `DocumentsV1Controller`/`Application/Integrations/Documents` query, not a new module.
- **Scope size estimate:** Very small — 1 new query (`GetDocumentByIdQuery`) + extending the existing list
  query with pagination/category/search parameters (likely a query-object change, not a new aggregate).
- **Suggested approach:** Small addition to existing `Integrations.Documents` module. Highest value-per-effort
  of all 9 — do this one first or near-first.
- **Dependencies/risks:** Low risk. Confirm the existing `GetDocumentsRequest`/`DocumentsQueryRequest` model
  can be additive-extended with pagination/filter fields without breaking the V1/V2 contract split (check
  `PlantSafety.Api/Integrations/Documents/Contracts/Requests/GetDocumentsRequest.cs` and
  `Models/Requests/DocumentsQueryRequest.cs` before extending — there may already be two divergent request
  shapes across V1/V2 to reconcile).

---

## 7. Reporting

**ApiClient:** `PlantSafety.UI/Modules/Reporting/ApiClients/ReportingApiClient.cs`
**Pages:** `ReportsListPage.razor`, `ReportDetailPage.razor`

**What's real today:** Two existing "reporting" surfaces neither of which match: `v1/analytics/reporting`
(`PlantSafety.Application/Analytics/Reporting`) and `v1/integrations/reporting`
(`PlantSafety.Api/Integrations/Reporting`, structurally identical in maturity to `Integrations/Documents` —
Authorization/Contracts/Diagnostics/Endpoints/Filters/Mapping/Models/Telemetry). Both are single-GET, neither
paginated/type-filtered/by-id.

**What's missing:** Paginated + type-filtered report list, and a report-detail-by-id endpoint. 2 endpoints.

- **Reuse vs. new-build:** Ambiguous reuse target — **two** existing candidates (`Analytics.Reporting` vs.
  `Integrations.Reporting`) rather than one clean match like DocumentManagement had. Given the UI's route is
  `v1/reporting/reports` (generic, not analytics- or integration-specific), and the page content (a generic
  reports list + detail) reads as "operational reports the user browses," `Integrations.Reporting` is the
  better structural fit (comparable to `Integrations.Documents`, which is the confirmed good-fit precedent for
  DocumentManagement). `Analytics.Reporting` is more likely to be about generating analytics reports, not
  browsing a document-like report list. Needs a one-line product/architecture confirmation but is not a hard
  ambiguity.
- **Scope size estimate:** Very small — same shape as DocumentManagement: 1 new by-id query + pagination/filter
  extension to an existing list query.
- **Suggested approach:** Small addition to `Integrations.Reporting`, following the same pattern recommended
  for DocumentManagement (#6). Do not extend `Analytics.Reporting` unless investigation of what that module's
  "report" actually represents contradicts this.
- **Dependencies/risks:** Genuine ambiguity flagged per the operating rule: which of the two existing
  "reporting" surfaces is the intended reuse target is a real judgment call, not fully resolvable from route
  names alone. Recommend a 10-minute reconciliation conversation before implementation to avoid building
  against the wrong one.

---

## 8. SafetyPrograms

**ApiClient:** `PlantSafety.UI/Modules/SafetyPrograms/ApiClients/SafetyProgramsApiClient.cs`
**Pages:** `SafetyProgramsListPage.razor`, `SafetyProgramDetailPage.razor`

**What's real today:** Nothing at `v1/safety-programs`. But the platform has three large, mature, closely
related modules: `SafetyPolicies` (94 files, 17 sub-domains: ConfinedSpaceEntry, HotWork, PPE, WorkingAtHeights,
etc.), `SafetyProcedures` (155 files, 10 sub-domains: LotoProcedures, EmergencyResponsePlanning, etc.), and
`SafetyCulture` (66 files, CultureAssessment). None of these expose a "Program" concept (a named
initiative/campaign with a status and a list of associated policies/procedures/training) — they're all
individually-scoped policy/procedure documents, not "programs" that bundle multiple things.

**What's missing:** Paginated + status-filtered program list, program detail. 2 endpoints — small surface, but
the underlying domain concept ("Safety Program" as an umbrella container) is new.

- **Reuse vs. new-build:** Small new aggregate, but should be built as a **thin composition layer** over
  `SafetyPolicies`/`SafetyProcedures`/`SafetyCulture`/Training rather than a duplicate document-authoring
  system. A "Safety Program" (e.g., "Fall Protection Program," "Hot Work Program") in real EHS practice is
  exactly a named bundle of policies + procedures + training requirements + a status — the platform already
  has all the leaf content types, it just lacks the grouping/rollup aggregate.
- **Scope size estimate:** Small-medium: 1 new lightweight aggregate (`SafetyProgram` — name, status, and
  references/links to existing Policy/Procedure/Training ids) + list/detail queries that hydrate the
  referenced items from the existing modules. Not a duplicate of 94+155+66 files of policy/procedure content —
  should be well under 15 new files if scoped correctly as a composition layer.
- **Suggested approach:** New minimal `SafetyPrograms` Application module whose detail query fans out to
  `SafetyPolicies`/`SafetyProcedures`/`SafetyCulture`/Training repositories for referenced content rather than
  storing program content directly. Get this scoping decision explicit before building — the failure mode to
  avoid is someone building "Program" as yet another standalone document type, which would be the 4th
  overlapping policy-like module.
- **Dependencies/risks:** Requires deciding the association model (does a Program reference existing Policy/
  Procedure ids, or duplicate/snapshot their content?) — a real design decision with data-integrity
  implications (what happens to a Program's snapshot when the underlying Policy changes?).

---

## 9. Routing

**ApiClient:** `PlantSafety.UI/Modules/Routing/ApiClients/RoutingApiClient.cs`
**Pages:** `RoutingDashboardPage.razor`, `RoutingDetailPage.razor`, `RoutingStepsPage.razor`,
`RoutingVersionHistoryPage.razor`, `RoutingDiffViewerPage.razor`, `RoutingSimulationPage.razor`, `RoutingApprovalPage.razor`

**What's real today:** No process-routing controller/Application module. The only "Routing" hits elsewhere in
the codebase are unrelated infra option classes (data-pipeline routing, exception routing) — confirmed false
friends, not relevant. However, `PlantSafety.Application/ProcessManagement` (`ProcessDefinition` aggregate)
is a strong conceptual analog: it already has `Create/Revise/SubmitForReview/Approve/Retire/Archive`
commands — i.e., a full versioning + approval-workflow lifecycle for a process-like entity — just not
"routing" specifically (ordered steps, step diffing, what-if simulation).

**What's missing:** Routing list, routing detail, ordered steps, version-history timeline, server-computed
diff between two versions, simulation run, and approval-workflow state. 7 endpoints, most conceptually novel
being the diff and simulation endpoints (real computation logic, not just CRUD).

- **Reuse vs. new-build:** Partial reuse. The **approval-workflow and version-history** pages map almost
  exactly onto `ProcessManagement`'s existing revise/submit-for-review/approve/retire lifecycle — recommend
  extending `ProcessDefinition` (or a new `ProcessRouting` aggregate that reuses the same lifecycle/versioning
  pattern) rather than reinventing approval workflow from scratch. The **steps, diff, and simulation**
  concepts are genuinely new — `ProcessManagement` currently has no notion of ordered steps within a process
  definition at all (its Commands only manage the definition's lifecycle status, not structural content).
- **Scope size estimate:** Medium. Reuse: approval workflow state (near-zero new code if `ProcessDefinition`'s
  existing status lifecycle is exposed as-is). New: `RoutingStep` child entity + ordering (~2-3 commands),
  version snapshot storage for diffing (this implies routings must be immutably versioned, which
  `ProcessDefinition`'s `Revise` command suggests already happens at the definition level — verify it captures
  full step-level snapshots, not just metadata), a diff algorithm (step-level added/removed/changed
  comparison — real logic, not CRUD), and a simulation engine (critical-path/bottleneck computation over the
  step graph — explicitly noted in the ApiClient's own header comment as logic that must live server-side,
  "no critical-path/bottleneck logic in UI").
- **Suggested approach:** Extend `ProcessManagement`'s `ProcessDefinition` aggregate with an ordered
  `RoutingStep` collection and step-level versioning, reuse its existing approval/revision lifecycle wholesale,
  and treat diff + simulation as two clearly separable, harder sub-epics on top (diff is moderate, simulation
  is the hardest single piece of new logic in this whole 9-module gap set alongside ProductionTracking's
  bottleneck detection).
- **Dependencies/risks:** Simulation requires defined semantics for "critical path" / "bottleneck" in a
  routing context that don't currently exist anywhere in the platform — this is closer to an algorithm design
  task than a CQRS build-out and should be scoped/estimated separately from the rest of the module.

---

## Recommended build order (business value / build cost)

Ranked highest ROI (do first) to lowest, weighing: endpoint count, whether real logic already exists to
extend, and whether the domain concept itself is settled or still ambiguous.

1. **DocumentManagement** — 2 endpoints, clean rewire onto existing mature `Integrations.Documents` module.
   Lowest cost, immediate fix, no open design questions.
2. **Reporting** — 2 endpoints, near-identical rewire pattern to #1, onto `Integrations.Reporting`. One small
   ambiguity (which of two existing "reporting" surfaces) to resolve first, otherwise same cost profile as #1.
3. **SafetyPrograms** — small new composition-layer aggregate over three already-mature policy/procedure/culture
   modules; high business value (safety-program tracking is core EHS work) for modest incremental cost.
4. **IncidentManagement** — base list/detail already works; the 5 lifecycle commands are a small, high-value
   addition to an aggregate that already exists. SLA monitor is a distinct, larger follow-on — ship lifecycle
   commands first, scope SLA engine as its own epic.
5. **PredictiveMaintenance** — genuine rewire target (`Operational.PredictiveMaintenance` already exists with
   real controllers); medium cost to expand 1 endpoint into 13, but building on a real foundation instead of
   from zero, and predictive-maintenance is high business value for a plant-safety platform.
6. **Routing** — medium cost, partial reuse of `ProcessManagement`'s lifecycle/versioning; the diff/simulation
   pieces are real new algorithmic work that will dominate the schedule, but approval/version-history reuse
   lowers the floor.
7. **Maintenance** — medium-large cost (new WorkOrder aggregate, parts, technician workload, calendar) with
   partial reuse of PM/Predictive Maintenance for two sub-resources; high business value (this is the module
   most plant floor users will touch daily) offsetting the build size.
8. **Scheduling** — cost is contingent on an unresolved design question (should "Job" be its own aggregate or
   an aggregation layer over Maintenance/PM/Drill scheduling); recommend sequencing after Maintenance (#7)
   lands so Scheduling can be built as a calendar/rollup layer rather than a duplicate concept, which changes
   its true cost substantially (could be small if done second, medium-large if done standalone/first).
9. **ProductionTracking** — largest, most novel scope (13 endpoints, new KPI/bottleneck-detection engine, no
   existing aggregate to extend beyond a single telemetry stream processor). Highest business value if the
   plant genuinely wants live production visibility, but also the highest cost and highest risk of hidden
   scope (ingestion-side schema gaps) of all 9 — recommend doing this last and only after a dedicated
   technical-design pass, not opportunistically alongside the others.

## One-line verdict per module

1. **IncidentManagement** — small addition (lifecycle commands to existing aggregate) + separate new-build (SLA engine).
2. **ProductionTracking** — new-build, largest and riskiest scope of the 9.
3. **Maintenance** — new-build for core (work orders/parts/calendar), with reuse of PM/Predictive Maintenance for two sub-resources.
4. **PredictiveMaintenance** — reuse/rewire (extend existing `Operational.PredictiveMaintenance`), medium expansion.
5. **Scheduling** — new-build, but should be re-scoped as a rollup layer over Maintenance/PM/Drill scheduling rather than an independent aggregate.
6. **DocumentManagement** — reuse/rewire (extend existing `Integrations.Documents`), smallest scope.
7. **Reporting** — reuse/rewire (extend existing `Integrations.Reporting`, pending a quick disambiguation vs. `Analytics.Reporting`).
8. **SafetyPrograms** — small addition as a new composition layer over existing SafetyPolicies/SafetyProcedures/SafetyCulture.
9. **Routing** — partial reuse (approval/versioning via `ProcessManagement`) + new-build for steps/diff/simulation.
