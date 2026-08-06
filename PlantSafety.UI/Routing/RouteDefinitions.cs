// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical strongly-typed route constant definitions for all
//   PlantSafety UI modules. Components and NavMenu MUST use these constants
//   exclusively — no magic strings in @page directives or NavLink hrefs.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

namespace PlantSafety.UI.Routing;

/// <summary>
/// Canonical route string constants for every page in the PlantSafety UI.
/// All @page directives and NavLink hrefs MUST reference these constants.
/// Adding new routes is additive; existing routes MUST NOT be renamed or removed.
/// </summary>
public static class RouteDefinitions
{
    // ── Shell ─────────────────────────────────────────────────────────────────

    /// <summary>Application home / dashboard.</summary>
    public const string Home = "/";

    /// <summary>Login landing page.</summary>
    public const string Login = "/login";

    /// <summary>Post-sign-out landing page.</summary>
    public const string SignedOut = "/signed-out";

    /// <summary>Administrative Setup page.</summary>
    public const string AdminSetup = "/admin/setup";

    // ── Admin Portal (Phase 7 §6.1 Admin Portal) ─────────────────────────────

    /// <summary>Admin Portal tenant management list.</summary>
    public const string AdminTenantList = "/admin/tenants";

    /// <summary>Admin Portal tenant detail (lifecycle actions, billing, support). Requires {tenantId} route parameter.</summary>
    public const string AdminTenantDetail = "/admin/tenants/{tenantId}";

    /// <summary>Admin Portal guided tenant provisioning wizard.</summary>
    public const string AdminTenantProvision = "/admin/tenants/provision";

    /// <summary>Admin Portal Support Console (tenant quick-links, impersonation, audit/event viewers).</summary>
    public const string AdminSupportConsole = "/admin/support";

    /// <summary>Admin Portal customer-onboarding quick-start checklist.</summary>
    public const string AdminOnboardingChecklist = "/admin/onboarding";

    /// <summary>Admin Portal per-tenant user management (list/create). Requires {tenantId} route parameter.</summary>
    public const string AdminTenantUsers = "/admin/tenants/{tenantId}/users";

    /// <summary>Admin/Executive Global Safety Dashboard (Phase 8 §5 Multi-Site Safety Intelligence Network).</summary>
    public const string AdminGlobalSafetyDashboard = "/admin/global-safety-dashboard";

    // ── Sub Menu (Section landing pages) ─────────────────────────────────────

    /// <summary>Core Operational sub menu.</summary>
    public const string MenuCoreOperational = "/menu/core-operational";

    /// <summary>Safety Event sub menu.</summary>
    public const string MenuSafetyEvent = "/menu/safety-event";

    /// <summary>Human Factors sub menu.</summary>
    public const string MenuHumanFactors = "/menu/human-factors";

    /// <summary>Operational Safety sub menu.</summary>
    public const string MenuOperationalSafety = "/menu/operational-safety";

    /// <summary>Analytics sub menu.</summary>
    public const string MenuAnalytics = "/menu/analytics";

    /// <summary>Compliance sub menu.</summary>
    public const string MenuCompliance = "/menu/compliance";

    /// <summary>Financial sub menu.</summary>
    public const string MenuFinancial = "/menu/financial";

    /// <summary>Intelligence sub menu.</summary>
    public const string MenuIntelligence = "/menu/intelligence";

    /// <summary>Planning Projects sub menu.</summary>
    public const string MenuPlanningProjects = "/menu/planning-projects";

    // ── Section 1 — Core Operational ─────────────────────────────────────────

    /// <summary>Alerts list page.</summary>
    public const string AlertsList = "/modules/alerts";

    /// <summary>Alert detail page. Requires {id} route parameter.</summary>
    public const string AlertDetail = "/modules/alerts/{id}";

    /// <summary>New Alert (Trigger) form.</summary>
    public const string AlertCreate = "/modules/alerts/new";

    /// <summary>Auditing list.</summary>
    public const string AuditingList = "/modules/auditing";

    /// <summary>Audit timeline viewer. Requires {EntityType}/{EntityId} route parameters (Phase 5 §6.1).</summary>
    public const string AuditTimeline = "/modules/auditing/timeline";

    /// <summary>Audit report generation/export/download page (Phase 5 §6.2).</summary>
    public const string AuditReports = "/modules/auditing/reports";

    /// <summary>New audit creation form.</summary>
    public const string AuditCreate = "/modules/auditing/new";

    /// <summary>Worker management list.</summary>
    public const string WorkerManagementList = "/modules/worker-management";

    /// <summary>Machine management list.</summary>
    public const string MachineManagementList = "/modules/machine-management";

    /// <summary>Process management list.</summary>
    public const string ProcessManagementList = "/modules/process-management";

    /// <summary>Training catalog.</summary>
    public const string TrainingList = "/modules/training";

    /// <summary>Certifications list.</summary>
    public const string CertificationsList = "/modules/certifications";

    /// <summary>New (generic) certification creation form.</summary>
    public const string CertificationCreate = "/modules/certifications/new";

    /// <summary>Generic Certification record detail page (distinct from the per-category tabbed detail). Requires {certificationId} route parameter.</summary>
    public const string CertificationRecordDetail = "/modules/certifications/record/{certificationId}";

    /// <summary>PPE management.</summary>
    public const string PpeList = "/modules/ppe";

    /// <summary>Safety culture dashboard.</summary>
    public const string CultureDashboard = "/modules/culture";

    /// <summary>Scheduling list.</summary>
    public const string SchedulingList = "/modules/scheduling";

    /// <summary>Maintenance list.</summary>
    public const string MaintenanceList = "/modules/maintenance";

    /// <summary>Create Work Order input form (Operational.WorkOrders write surface — CreateWorkOrderCommand).</summary>
    public const string MaintenanceCreateWorkOrder = "/modules/maintenance/work-orders/new";

    /// <summary>Work Order detail page. Requires {WorkOrderId} route parameter.</summary>
    public const string MaintenanceWorkOrderDetail = "/modules/maintenance/work-orders/{WorkOrderId}";

    /// <summary>Predictive maintenance dashboard.</summary>
    public const string PredictiveMaintenanceDashboard = "/modules/predictive-maintenance";

    /// <summary>Raise a new PredictiveMaintenanceAlert (Operational.PredictiveMaintenance write surface).</summary>
    public const string PredictiveMaintenanceAlertCreate = "/modules/predictive-maintenance/alerts/new";

    /// <summary>Find a PredictiveMaintenanceAlert by id (no list endpoint exists for alerts — this is the real, reachable lookup surface).</summary>
    public const string PredictiveMaintenanceAlertFind = "/modules/predictive-maintenance/alerts/find";

    /// <summary>PredictiveMaintenanceAlert detail page. Requires {alertId} route parameter.</summary>
    public const string PredictiveMaintenanceAlertDetail = "/modules/predictive-maintenance/alerts/{alertId}";

    /// <summary>Production tracking dashboard.</summary>
    public const string ProductionTrackingDashboard = "/modules/production-tracking";

    /// <summary>Routing configuration.</summary>
    public const string RoutingList = "/modules/routing";

    /// <summary>Reporting list.</summary>
    public const string ReportingList = "/modules/reporting";

    /// <summary>Safety programs list.</summary>
    public const string SafetyProgramsList = "/modules/safety-programs";

    /// <summary>New safety program creation form (CreateSafetyProgramCommand).</summary>
    public const string SafetyProgramsCreate = "/modules/safety-programs/new";

    /// <summary>Assign a new training record to a worker (AssignTrainingCommand — the real, controller-wired write surface at api/v1/features/operational/training, distinct from the read-only Operational Training list this module's list/detail pages already render).</summary>
    public const string TrainingAssign = "/modules/training/new";

    /// <summary>Incident management list.</summary>
    public const string IncidentManagementList = "/modules/incident-management";

    /// <summary>Incident management — new incident (Create).</summary>
    public const string IncidentManagementCreate = "/modules/incident-management/new";

    /// <summary>Document management list.</summary>
    public const string DocumentManagementList = "/modules/document-management";

    /// <summary>Workflow approvals inbox (Phase 5 §5 — pending HumanApproval steps for the current actor).</summary>
    public const string WorkflowApprovalsInbox = "/modules/workflow/approvals";

    /// <summary>Workflow instance detail/timeline. Requires {id} route parameter.</summary>
    public const string WorkflowInstanceDetail = "/modules/workflow/{id}";

    // ── Section 2 — Safety Event ──────────────────────────────────────────────

    /// <summary>Crisis management dashboard.</summary>
    public const string CrisisManagementDashboard = "/modules/crisis-management";

    /// <summary>Drills and exercises list.</summary>
    public const string DrillsList = "/modules/drills";

    /// <summary>New (schedule) drill form.</summary>
    public const string DrillsCreate = "/modules/drills/new";

    /// <summary>Emergency management dashboard.</summary>
    public const string EmergencyManagementDashboard = "/modules/emergency-management";

    /// <summary>Near miss list.</summary>
    public const string NearMissList = "/modules/near-miss";

    /// <summary>New near miss form.</summary>
    public const string NearMissCreate = "/modules/near-miss/new";

    /// <summary>Observation list.</summary>
    public const string ObservationList = "/modules/observations";

    /// <summary>New observation form.</summary>
    public const string ObservationCreate = "/modules/observations/new";

    /// <summary>Root cause analysis list.</summary>
    public const string RcaList = "/modules/rca";

    /// <summary>Create RCA linked to an Incident/NearMiss (Features.SafetyEvents.RootCauseAnalysis write surface).</summary>
    public const string RcaCreate = "/modules/rca/new";

    /// <summary>ContinuousImprovement.RCA generic records list (real List/Search endpoint — distinct aggregate from the Incident/NearMiss-linked RCA above).</summary>
    public const string RcaRecordsList = "/modules/rca/records";

    /// <summary>Create a generic ContinuousImprovement.RCA record.</summary>
    public const string RcaRecordCreate = "/modules/rca/records/new";

    /// <summary>ContinuousImprovement.RCA generic record detail page. Requires {rcaId} route parameter.</summary>
    public const string RcaRecordDetail = "/modules/rca/records/{rcaId}";

    // ── Section 3 — Human Factors ─────────────────────────────────────────────

    /// <summary>Human factors dashboard.</summary>
    public const string HumanFactorsDashboard = "/modules/human-factors";

    /// <summary>Fatigue management dashboard.</summary>
    public const string FatigueManagementDashboard = "/modules/fatigue-management";

    /// <summary>Ergonomics assessment list.</summary>
    public const string ErgonomicsList = "/modules/ergonomics";

    /// <summary>Industrial hygiene list.</summary>
    public const string IndustrialHygieneList = "/modules/industrial-hygiene";

    /// <summary>Worker wellbeing dashboard.</summary>
    public const string WorkerWellBeingDashboard = "/modules/worker-wellbeing";

    // ── Section 4 — Operational Safety ───────────────────────────────────────

    /// <summary>Equipment safety list.</summary>
    public const string EquipmentSafetyList = "/modules/equipment-safety";

    /// <summary>Hazard assessment list.</summary>
    public const string HazardAssessmentList = "/modules/hazard-assessment";

    /// <summary>HazMat management list.</summary>
    public const string HazMatList = "/modules/hazmat";

    /// <summary>Inspections list.</summary>
    public const string InspectionsList = "/modules/inspections";

    /// <summary>Job Safety Analysis list.</summary>
    public const string JsaList = "/modules/jsa";

    /// <summary>LOTO procedures list.</summary>
    public const string LotoList = "/modules/loto";

    /// <summary>Hazardous Waste Management — Hazardous Waste Lifecycle Tracker (open manifests).</summary>
    public const string HazardousWasteManagementManifests = "/modules/hazardous-waste-management";

    /// <summary>Hazardous Waste Management — Accumulation Area Time-Limit Board.</summary>
    public const string HazardousWasteManagementAccumulationAreas = "/modules/hazardous-waste-management/accumulation-areas";

    /// <summary>Water Discharge Compliance — Discharge Compliance Status.</summary>
    public const string WaterDischargeComplianceStatus = "/modules/environmental-health/water-discharge-compliance";

    /// <summary>Physical Access Control — Anomaly Feed (main dashboard).</summary>
    public const string PhysicalAccessControlDashboard = "/modules/physical-access-control";

    /// <summary>Logistics &amp; Transport Safety dashboard.</summary>
    public const string LogisticsTransportSafetyDashboard = "/modules/logistics-transport-safety";

    /// <summary>Community Safety dashboard.</summary>
    public const string CommunitySafetyDashboard = "/modules/community-safety";

    /// <summary>Public Safety Communication dashboard.</summary>
    public const string PublicSafetyCommunicationDashboard = "/modules/public-safety-communication";

    /// <summary>Cyber-Physical Security dashboard.</summary>
    public const string CyberPhysicalSecurityDashboard = "/modules/cyber-physical-security";

    /// <summary>Autonomous Vehicle Safety dashboard.</summary>
    public const string AutonomousVehicleSafetyDashboard = "/modules/autonomous-vehicle-safety";

    /// <summary>Machine Safety — Crane/Hoist Systems Fleet Status.</summary>
    public const string CraneHoistSystemsFleetStatus = "/modules/machine-safety/crane-hoist-systems";

    /// <summary>Local Emergency Coordination dashboard.</summary>
    public const string LocalEmergencyCoordinationDashboard = "/modules/local-emergency-coordination";

    /// <summary>Sustainability — Advanced Metrics dashboard.</summary>
    public const string SustainabilityAdvancedMetricsDashboard = "/modules/sustainability/advanced-metrics";

    /// <summary>Human Factors — Cognitive Load Analytics dashboard.</summary>
    public const string CognitiveLoadAnalyticsDashboard = "/modules/human-factors/cognitive-load-analytics";

    /// <summary>Digital Twin — Evacuation Modeling readiness board.</summary>
    public const string EvacuationModelingReadinessBoard = "/modules/digital-twin/evacuation-modeling/readiness-board";

    /// <summary>Permits to Work list.</summary>
    public const string PermitsList = "/modules/permits";

    /// <summary>Create generic Permit (Compliance.Permits write surface).</summary>
    public const string PermitCreate = "/modules/permits/new";

    /// <summary>Generic Permit record detail page (distinct from the Operational.PermitsToWork read-only detail). Requires {permitsId} route parameter.</summary>
    public const string PermitRecordDetail = "/modules/permits/record/{permitsId}";

    // ── Section 5 — Analytics ─────────────────────────────────────────────────

    /// <summary>Culture analytics dashboard.</summary>
    public const string CultureAnalyticsDashboard = "/modules/culture-analytics";

    /// <summary>Dashboard analytics overview.</summary>
    public const string DashboardAnalytics = "/modules/dashboard-analytics";

    /// <summary>Engagement analytics dashboard.</summary>
    public const string EngagementAnalyticsDashboard = "/modules/engagement-analytics";

    /// <summary>Predictive analytics dashboard.</summary>
    public const string PredictiveAnalyticsDashboard = "/modules/predictive-analytics";

    /// <summary>Reporting analytics list.</summary>
    public const string ReportingAnalyticsList = "/modules/reporting-analytics";

    /// <summary>Scheduling analytics dashboard.</summary>
    public const string SchedulingAnalyticsDashboard = "/modules/scheduling-analytics";

    /// <summary>Scorecards dashboard.</summary>
    public const string ScorecardsDashboard = "/modules/scorecards";

    /// <summary>Trend analysis dashboard.</summary>
    public const string TrendAnalysisDashboard = "/modules/trend-analysis";

    /// <summary>BI Dashboards (Phase 5 §4.3 Scheduling/Production/Maintenance/Risk/ContinuousImprovement/Audit KPI sub-features).</summary>
    public const string BiDashboards = "/modules/bi-dashboards";

    // ── Section 6 — Compliance ────────────────────────────────────────────────

    /// <summary>Compliance dashboard.</summary>
    public const string ComplianceDashboard = "/modules/compliance";

    /// <summary>Management of Change list.</summary>
    public const string MocList = "/modules/moc";

    /// <summary>New Management of Change request form.</summary>
    public const string MocCreate = "/modules/moc/new";

    /// <summary>Policies and procedures list.</summary>
    public const string PoliciesList = "/modules/policies";

    /// <summary>Create generic Policy (Compliance.Policy write surface).</summary>
    public const string PolicyRecordCreate = "/modules/policies/new";

    /// <summary>Generic Policy record detail page (distinct from the read-only Policy/Procedure detail pages). Requires {policyId} route parameter.</summary>
    public const string PolicyRecordDetail = "/modules/policies/record/{policyId}";

    /// <summary>Regulatory calendar.</summary>
    public const string RegulatoryCalendar = "/modules/regulatory-calendar";

    /// <summary>Create generic RegulatoryCalendar obligation (Compliance.RegulatoryCalendar write surface).</summary>
    public const string RegulatoryCalendarRecordCreate = "/modules/regulatory-calendar/new";

    /// <summary>Generic RegulatoryCalendar obligation record detail page (distinct from the read-only per-source detail page). Requires {regulatoryCalendarId} route parameter.</summary>
    public const string RegulatoryCalendarRecordDetail = "/modules/regulatory-calendar/record/{regulatoryCalendarId}";

    /// <summary>Compliance Rules Engine evaluation trigger + results view (Phase 5 §6.3).</summary>
    public const string ComplianceEvaluation = "/modules/compliance/evaluation";

    // ── Section 7 — Financial ─────────────────────────────────────────────────

    /// <summary>Financials dashboard.</summary>
    public const string FinancialsDashboard = "/modules/financials";

    /// <summary>ROI analysis dashboard.</summary>
    public const string RoiDashboard = "/modules/roi";

    /// <summary>Create Budget input form (Financials).</summary>
    public const string FinancialsCreateBudget = "/modules/financials/budgets/new";

    /// <summary>Record Cost (FinancialRecord create) input form (Financials).</summary>
    public const string FinancialsRecordCost = "/modules/financials/costs/new";

    /// <summary>Calculate/Record ROI input form.</summary>
    public const string RoiCalculate = "/modules/roi/calculate";

    /// <summary>Checklist list.</summary>
    public const string ChecklistList = "/modules/checklists";

    /// <summary>New checklist template creation/management workspace.</summary>
    public const string ChecklistCreate = "/modules/checklists/new";

    /// <summary>Contractor management list.</summary>
    public const string ContractorManagementList = "/modules/contractor-management";

    // ── Section 8 — Intelligence ──────────────────────────────────────────────

    /// <summary>AI training dashboard.</summary>
    public const string AiTrainingDashboard = "/modules/ai-training";

    /// <summary>AI investigation list.</summary>
    public const string AiInvestigationList = "/modules/ai-investigation";

    /// <summary>New AI investigation form.</summary>
    public const string AiInvestigationCreate = "/modules/ai-investigation/new";

    /// <summary>Data lake explorer.</summary>
    public const string DataLakeExplorer = "/modules/data-lake";

    /// <summary>Predictive safety dashboard.</summary>
    public const string PredictiveSafetyDashboard = "/modules/predictive-safety";

    /// <summary>Exports list.</summary>
    public const string ExportsList = "/modules/exports";

    /// <summary>Governance and access control.</summary>
    public const string GovernanceDashboard = "/modules/governance";

    /// <summary>Digital twin viewer.</summary>
    public const string DigitalTwinViewer = "/modules/digital-twin";

    /// <summary>Sustainability dashboard.</summary>
    public const string SustainabilityDashboard = "/modules/sustainability";

    /// <summary>AI governance dashboard.</summary>
    public const string AiGovernanceDashboard = "/modules/ai-governance";

    /// <summary>Enterprise fabric configuration.</summary>
    public const string EnterpriseFabricDashboard = "/modules/enterprise-fabric";

    /// <summary>Environmental health dashboard.</summary>
    public const string EnvironmentalHealthDashboard = "/modules/environmental-health";

    // ── Section 9 — Planning Projects ────────────────────────────────────────

    /// <summary>Planning projects list.</summary>
    public const string PlanningProjectsList = "/modules/planning-projects";

    /// <summary>Create Planning Project input form. Covers Plans/Projects/Scheduling data entry
    /// (a Project's TargetCompletionUtc IS the schedule input — there is no separate
    /// scheduling-create endpoint).</summary>
    public const string PlanningProjectsCreate = "/modules/planning-projects/new";

    /// <summary>Add Milestone input form. Requires {ProjectId} route parameter.</summary>
    public const string PlanningProjectsAddMilestone = "/modules/planning-projects/{ProjectId}/milestones/new";

    /// <summary>Project budgeting dashboard.</summary>
    public const string ProjectBudgetingDashboard = "/modules/project-budgeting";

    /// <summary>Project scheduling dashboard.</summary>
    public const string ProjectSchedulingDashboard = "/modules/project-scheduling";

    // ── Operations ────────────────────────────────────────────────────────────

    /// <summary>Operations backup/DR dashboard (Phase 12 + Phase 18 SQL Server backup automation).</summary>
    public const string OperationsBackupDashboard = "/modules/operations-backup";

    /// <summary>Operations schema drift guard dashboard (Phase 7 Governance [System].[SchemaMigrations] tracking).</summary>
    public const string OperationsSchemaGuardDashboard = "/modules/operations-schema-guard";

    /// <summary>Operations archival/purge dashboard (Phase 7 Governance retention/purge automation).</summary>
    public const string OperationsArchivalDashboard = "/modules/operations-archival";

    /// <summary>Operations audit viewer dashboard (shared cross-database dbo.AuditLog search/export/diff view).</summary>
    public const string OperationsAuditViewerDashboard = "/modules/operations-audit-viewer";

    /// <summary>Operations monitoring dashboard (DB Health, DB Size, DB Performance, DB Errors, DB Deadlocks).</summary>
    public const string OperationsMonitoringDashboard = "/modules/operations-monitoring";

    /// <summary>Operations configuration dashboard (Phase 14 Governance [System].[FeatureFlag] editor; feature flags only — see page for Thresholds/Routing Rules/Tenant Overrides scope note).</summary>
    public const string OperationsConfigurationDashboard = "/modules/operations-configuration";

    /// <summary>Operations System Health rollup dashboard (Step 10/10: unified view over Backup/SchemaGuard/Monitoring/Archival/AuditViewer/Configuration health plus app/DB version and Task Scheduler status).</summary>
    public const string OperationsSystemHealthDashboard = "/modules/operations-system-health";

    // ── Diagnostics (Part XI-A) ───────────────────────────────────────────────

    /// <summary>DiagnosticsViewer main page.</summary>
    public const string DiagnosticsViewer = "/modules/diagnostics";

    /// <summary>DiagnosticsViewer comparison view.</summary>
    public const string DiagnosticsComparison = "/modules/diagnostics/compare";

    /// <summary>DiagnosticsViewer fault viewer.</summary>
    public const string DiagnosticsFaults = "/modules/diagnostics/faults";
}
