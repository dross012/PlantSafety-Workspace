# Authorization Tiers and Controller Consistency (PlantSafety + Reporting)

**Date:** 2026-07-27
**Author:** Security & Compliance Agent (full authorization consistency pass)

This document covers the canonical Safety-domain authorization tier model,
the controller-to-tier mapping, and the cross-module consistency rules now
enforced between PlantSafety.Api and Reporting.Api. It does not duplicate
`docs/architecture/audit-logging.md` or `plantsafety-architecture.md`.

Canonical source of truth for the tiers/policies themselves:
`C:\Users\dross\source\Shared\Authorization\Shared.Authorization.Abstractions\`
(`Roles\SafetyRoles.cs`, `Policies\SafetyTierPolicies.cs`).

## 1. Inventory (what was actually found)

- **363 controllers** total: 362 in `PlantSafety.Api` (reflection-discovered;
  see `PlantSafety.Tests/Api/Authorization/ControllerAuthorizationTierTests.cs`
  sanity check `HaveCountGreaterThan(300)`) + 1 in `Reporting.Api`
  (`ReportsController`, the only controller in that API).
- **~147** per-bounded-context `*AuthorizationPolicies` classes (e.g.
  `IncidentManagementAuthorizationPolicies`), each declaring a `Read`/`Write`
  policy-name-constant pair, used by the "modern" (Authorization/ folder,
  V1+V2) controllers.
- **57 "legacy" controllers** (no per-module `*AuthorizationPolicies` class)
  that had only `[Authorize(Policy = "RequireAuthenticatedUser")]` at the
  controller level — **no distinction between read and write, and no role
  check at all** beyond "is logged in". Full list: `AccidentCategoriesV1Controller`,
  `AccidentManagementV1Controller`, `AlertsV1Controller`, `AuditEngagementsV1Controller`,
  `AuditFindingsV1Controller`, `ChecklistCompletionsV1Controller`,
  `ChecklistTemplatesV1Controller`, `ContractorSafetyV1Controller`,
  `DashboardsV1Controller`, `WidgetsV1Controller`, `EmployeeSafetyV1Controller`,
  `AiGovernanceV1Controller`, `Features/Analytics/Dashboard/DashboardV1Controller`,
  `Features/Analytics/Reporting/ReportingV1Controller`, `ManagementOfChangeV1Controller`,
  `PermitsV1Controller`, `CorrectiveActionsV1Controller`, `DigitalTwinV1Controller`,
  `FabricNodesV1Controller`, `Features/EnterpriseFabric/Users/UsersV1Controller`,
  `EnvironmentalHealthReadingsV1Controller`, `BudgetsV1Controller`, `RoiV1Controller`,
  `Features/Operational/Training/TrainingV1Controller`, `ProjectsV1Controller`,
  `ProcessesV1Controller`, `CrisisEventsV1Controller`, `DrillsV1Controller`,
  `EmergencyResponsesV1Controller`, `Features/SafetyEvents/Exports/ExportsV1Controller`,
  `IncidentsV1Controller`, `Features/SafetyEvents/NearMisses/NearMissesV1Controller`,
  `Features/SafetyEvents/Observation/ObservationsV1Controller`,
  `Features/SafetyEvents/RootCauseAnalysis/RootCauseAnalysisV1Controller`,
  `SustainabilityV1Controller`, `WorkerWellBeingV1Controller`, `MachineSafetyV1Controller`,
  `ObservabilityV1Controller`, `OperationsArchivalV1Controller`,
  `OperationsAuditViewerV1Controller`, `OperationsBackupV1Controller`,
  `OperationsConfigurationV1Controller`, `OperationsMonitoringV1Controller`,
  `OperationsSchemaGuardV1Controller`, `OperationsSystemHealthV1Controller`,
  `PermitToWorkV1Controller`, `PPEV1Controller`, `RiskCategoriesV1Controller`,
  `RiskManagementV1Controller`, `CultureAssessmentsV1Controller`,
  `StopWorkEventsV1Controller`, `SafetyPoliciesV1Controller`,
  `SafetyPolicyCategoriesV1Controller`, `SafetyProceduresV1Controller`,
  `SchedulingV1Controller`, `ScorecardCategoriesV1Controller`, `ScorecardsV1Controller`.
- **8 additional controllers** (`AnalyticsExportV1Controller`,
  `ComplianceBulkV1Controller`, `ComplianceExportV1Controller`,
  `ContinuousImprovementExportV1Controller`, `OperationalBulkV1Controller`,
  `OperationalExportV1Controller`, `SafetyEventsBulkV1Controller`,
  `SafetyEventsExportV1Controller`) used a **third, orphaned authorization
  pattern**: raw string-literal policy names (`"SafetyEvents.Export"`,
  `"Compliance.Approve"`, etc.) that matched neither the per-module
  `*AuthorizationPolicies` constants nor the tier system.
- **3 controllers** legitimately `[AllowAnonymous]`: `PlatformV1Controller`,
  `PlatformV2Controller` (version/health info, no Safety data), and
  `WebhookReceiverV1Controller` (authenticates external callers via webhook
  signature, not a JWT bearer — see `WebhookAuthorizationPolicies.cs`).

## 2. Inconsistencies found

1. **Critical — the entire ~147-module policy system was non-functional.**
   `AuthorizationPolicyExtensions.AddPlantSafetyPolicies()` was declared but
   **never invoked** from `ServiceCollectionExtensions.AddPlantSafetyAuthorization()`
   (confirmed by grepping the whole tree for its only reference: its own
   declaration). It also only registered ~34 coarse `<Domain>.<Verb>` policy
   names that no controller ever referenced. The practical effect: every
   action using `[Authorize(Policy = "Require<Module><Verb>")]` (the majority
   of the ~300 "modern" controller actions) pointed at a policy name
   registered **nowhere**, which ASP.NET Core's authorization middleware
   treats as a hard `InvalidOperationException` on first request. This is a
   functional break, not merely weak security — but a broken-open-by-error
   posture is exactly the kind of failure Zero-Trust enforcement should never
   depend on.
2. **57 legacy controllers relied on UI-only / no server-side role
   enforcement** — every read (GET) and every state-changing action
   (including regulatory-relevant transitions like `close`, `approve`,
   `escalate`, `dispatch`, `resolve`) was reachable by any authenticated user
   regardless of role. This is the "weaker rules than sibling controllers
   doing the same kind of operation" pattern called out in the task: the
   ~147-module tier already separated Read/Write; these 57 did not separate
   anything.
3. **8 controllers referenced orphaned raw-string policy literals**
   (`"SafetyEvents.Export"`, `"Compliance.Update"`, etc.) that were never
   registered under any system, old or new.
4. **Mismatched role-name semantics for the same admin concept.**
   `AuthorizationPolicyExtensions.cs` used `"Platform.Admin"` as the
   universal admin-override role across all ~147 module policies; the
   separate, unused `RequireDashboardRead` policy in
   `ServiceCollectionExtensions.cs` used `"PlatformAdministrator"` for what
   is evidently the same role. Canonicalized to a single literal.
5. **Reporting.Api had zero authentication or authorization at all.**
   `ReportsController` (the only controller in that API) had no
   `[Authorize]` attribute anywhere, and `Program.cs` never registered
   `AddAuthentication`/`AddAuthorization` and never called
   `app.UseAuthentication()` (it called `app.UseAuthorization()`, which is a
   no-op with zero `[Authorize]` attributes present). Every
   generate/export/preview report endpoint — which serve incident,
   compliance, and other Safety-domain data — was reachable by anyone,
   unauthenticated, over the network. This is the single most severe finding
   of the pass, worse than any PlantSafety.Api controller (all of which
   required at least an authenticated caller).

## 3. Canonical tiers

Four tiers were derived from what the codebase's real role usage already
implied — not invented from scratch:

- The ~147-module system already used a consistent **Read / Write** pair
  per module (2 tiers).
- The domain-level policies (`Compliance.Approve`, `SafetyEvents.Approve`,
  `Operational.Approve`, `ContinuousImprovement.Approve`) already
  distinguished a separate, higher-privilege **Approve** verb from plain
  Create/Update.
- Every module policy already carved out **`Platform.Admin`** as a universal
  override role, and several controllers (backup, restore, schema-guard,
  system-health, webhook management) are inherently administrative
  regardless of HTTP verb.

Reconciling these three existing patterns gives four tiers — no more, no
fewer than the evidence supports:

| Tier | Role constant | Used for |
|---|---|---|
| **Viewer** | `SafetyRoles.Viewer` = `"Safety.Viewer"` | Read-only (GET) access |
| **Editor** | `SafetyRoles.Editor` = `"Safety.Editor"` | Standard create/update (POST/PUT) |
| **Approver** | `SafetyRoles.Approver` = `"Safety.Approver"` | State-transition operations with safety/regulatory weight: approve, close, escalate, dispatch, resolve, reject, cancel, complete, verify, publish, acknowledge, authorize, confirm; also data-egress/export operations |
| **PlatformAdmin** | `SafetyRoles.PlatformAdmin` = `"Platform.Admin"` | Full override; destructive (delete) and platform-management/backup/restore/schema/monitoring operations |

Role sets are cumulative (`SafetyRoles.ViewerAndAbove`, `EditorAndAbove`,
`ApproverAndAbove`, `AdminOnly`) — a PlatformAdmin can do everything a
Viewer can, etc.

Four canonical **policy** names
(`Shared.Authorization.Abstractions.Policies.SafetyTierPolicies`) map 1:1 to
the role sets above: `Read`, `Write`, `Approve`, `Admin` (backing string
values `"Safety.Tier.Read"` / `.Write` / `.Approve` / `.Admin`).

## 4. Controller-to-tier mapping

### 4.1 Modern per-module controllers (~147 modules, ~297 controllers)

Unchanged in shape (still `[Authorize(Policy = "Require<Module><Verb>")]`
via each module's own `*AuthorizationPolicies` class) but now **actually
registered**: `AuthorizationPolicyExtensions.AddPlantSafetyPolicies()` was
rewritten to reflect over every `*AuthorizationPolicies` class in the
assembly, read every public `const string` field, and register a matching
policy — inferring the tier from the field's own name:

| Field name | Tier | Role set |
|---|---|---|
| `Read` | Viewer | `ViewerAndAbove` |
| `Write` / `Create` / `Update` | Editor | `EditorAndAbove` |
| `Approve` / `Export` / `ViewLogs` | Approver | `ApproverAndAbove` |
| `Delete` / `Manage` / anything unrecognised | PlatformAdmin | `AdminOnly` |

New modules therefore get correctly-registered policies automatically —
this specific class of drift (a policy referenced by a controller but never
registered) cannot recur without a test failure (see §6).

### 4.2 Legacy controllers (57 controllers, patched this pass)

Class-level `[Authorize(Policy = "RequireAuthenticatedUser")]` retained
(still requires authentication) but every routed action now also carries an
explicit `[Authorize(Policy = SafetyTierPolicies.<Tier>)]` override, chosen
per action:

- `Operations/*` controllers (Archival, AuditViewer, Backup, Configuration,
  Monitoring, SchemaGuard, SystemHealth) — **every** action, GET included,
  requires `Admin`. These are infrastructure operations where even read
  visibility is administrator-only.
- All other controllers: `GET` → `Read`; `POST`/`PUT`/`DELETE`/`PATCH` whose
  method name contains an approve/close/escalate/dispatch/resolve/reject/
  cancel/complete/assign/verify/publish/reopen/reassign/acknowledge/
  authorize/confirm/finalize/start verb → `Approve`; everything else → `Write`.

### 4.3 Orphaned-literal controllers (8 controllers, patched this pass)

`AnalyticsExportV1Controller`, `ComplianceBulkV1Controller`,
`ComplianceExportV1Controller`, `ContinuousImprovementExportV1Controller`,
`OperationalBulkV1Controller`, `OperationalExportV1Controller`,
`SafetyEventsBulkV1Controller`, `SafetyEventsExportV1Controller`: raw
literals replaced 1:1 with `SafetyTierPolicies` constants (`.Export`/
`.Approve` → `Approve`; `.Update` → `Write`).

### 4.4 Reporting.Api (1 controller, patched this pass)

`ReportsController` — see
`Reporting.Api/Extensions/AuthorizationServiceCollectionExtensions.cs` for
the authentication registration that was entirely missing. Tiering:

| Action | Tier | Rationale |
|---|---|---|
| `GetReportTypes`, `GetReportMetadata` | `Read` (controller-level default) | Pure metadata, no egress |
| `GetReportPreview` | `Write` | Runs generation logic but produces no downloadable artifact |
| `GenerateReport`, `ExportReport` | `Approve` | Data-egress (downloadable file) of Safety-domain data — same tier as PlantSafety's own `Export`/`ViewLogs` policies (§5) |

## 5. Cross-module consistency rules (PlantSafety ↔ Reporting)

- **Shared source of truth:** both `PlantSafety.Api` and `Reporting.Api` now
  reference the same `Shared.Authorization.Abstractions` project
  (`Shared/Authorization/Shared.Authorization.Abstractions/`, created this
  pass, following the same layout convention as `Shared/AuditLogging`).
  Neither module may declare its own role-name or tier-policy string
  literals; both reference `SafetyRoles` / `SafetyTierPolicies` constants.
- **Rule: data-egress operations require Approver-or-above in both
  modules.** PlantSafety's own `Export`/`ViewLogs`-suffixed module policies
  resolve to `SafetyRoles.ApproverAndAbove` (§4.1); Reporting's
  `GenerateReport`/`ExportReport` (file-download endpoints) require
  `SafetyTierPolicies.Approve` — the identical shared constant. This is
  enforced by two independent regression tests referencing the same
  constant (§6), not by cross-repo compile-time coupling (PlantSafety and
  Reporting remain separate git repos with separate solutions; only the
  `Shared/` folder — already an established sibling-repo dependency pattern
  per `Shared/AuditLogging` — is shared).
- **Rule: read-only metadata never requires more than Viewer-equivalent.**
  Reporting's `GetReportTypes`/`GetReportMetadata` require the same `Read`
  tier as any PlantSafety GET endpoint.

## 6. Regression tests added

- `PlantSafety.Tests/Api/Authorization/AuthorizationPolicyRegistrationTests.cs`
  — builds the real `AuthorizationOptions` via
  `AuthorizationPolicyExtensions.AddPlantSafetyPolicies()` and asserts every
  `[Authorize(Policy = ...)]` string referenced anywhere in the ~362
  PlantSafety.Api controllers resolves to a registered policy. This is the
  direct regression guard for finding §2.1.
- `PlantSafety.Tests/Api/Authorization/ControllerAuthorizationTierTests.cs`
  — reflection over the full ~362-controller surface (not a hand-picked
  sample): (a) every routed action enforces a role-restricting policy or is
  on the reviewed `[AllowAnonymous]` allow-list; (b) per controller, GET
  actions never require a stricter tier than that controller's write
  actions.
- `PlantSafety.Tests/Api/Authorization/CrossModuleExportTierContractTests.cs`
  and `Reporting.Tests/Security/ReportsControllerAuthorizationTests.cs` —
  the two-sided cross-module contract from §5.

Full run at the time of writing: `PlantSafety.Tests` 723/723 passing,
`Reporting.Tests` 105/105 passing (both suites run clean, no regressions
introduced elsewhere in either repo).

## 7. Known follow-ups (flagged for manual review, not fixed here)

- `RequireDashboardRead` was confirmed dead: no controller referenced it
  (`DashboardsV1Controller`/`WidgetsV1Controller` use the canonical
  `SafetyTierPolicies.Read/Write/Approve` instead) and has been removed
  from `ServiceCollectionExtensions.cs`, along with the corresponding
  registration in the test harnesses that mirrored it.
- The `RequireRole(...)` policy builder pattern is explicitly documented in
  `AuthorizationPolicyExtensions.cs` as a placeholder for a future
  claim/permission-based model backed by the Roles bounded context — that
  design note predates this pass and is unchanged.
- Azure AD `Authority`/`Audience` values in `Reporting.Api/appsettings.json`
  are placeholders (empty strings), matching the fact that no JWT issuer
  configuration existed for this API before this pass. Must be populated
  with the real tenant values before deployment — flagged for manual
  operations follow-up, not something this pass can determine from source.
