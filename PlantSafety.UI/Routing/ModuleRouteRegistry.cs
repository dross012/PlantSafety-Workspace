// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Canonical, single-source-of-truth registry of all 66
//   PlantSafety v5.0 modules grouped into their 9 sections, plus the
//   non-counted Platform Tools section (Diagnostics Viewer). Consumed by
//   the Main Menu (section tiles), the 9 Sub Menu pages (module lists),
//   and NavMenu — so the sidebar, main menu, and sub menus can never drift
//   out of sync with one another. All Href values come from
//   RouteDefinitions constants exclusively — no magic strings.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

namespace PlantSafety.UI.Routing;

/// <summary>Describes one of the 9 v5.0 module sections (plus Platform Tools) and its member modules.</summary>
public sealed class ModuleSection
{
    /// <summary>Gets the kebab-case section code used in Sub Menu routes (e.g., "core-operational").</summary>
    public string SectionCode { get; init; } = string.Empty;

    /// <summary>Gets the display title shown on Main Menu tiles and Sub Menu headers.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets a short description shown on the Main Menu tile.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the Bootstrap Icons class name for the section tile.</summary>
    public string IconClass { get; init; } = string.Empty;

    /// <summary>Gets the Sub Menu route for this section, or null for sections without a Sub Menu (e.g., Platform Tools).</summary>
    public string? SubMenuHref { get; init; }

    /// <summary>Gets the modules belonging to this section, in display order.</summary>
    public IReadOnlyList<ModuleRoute> Modules { get; init; } = [];
}

/// <summary>
/// Canonical registry of all navigable PlantSafety modules. This is the single
/// source of truth for module metadata (label, icon, route, section) — NavMenu,
/// the Main Menu, and the Sub Menu pages all read from this registry.
/// </summary>
public static class ModuleRouteRegistry
{
    /// <summary>Gets the 9 v5.0 module sections plus the non-counted Platform Tools section, in display order.</summary>
    public static IReadOnlyList<ModuleSection> Sections { get; } = BuildSections();

    /// <summary>Gets a flattened view of every registered module across all sections.</summary>
    public static IReadOnlyList<ModuleRoute> AllModules { get; } =
        Sections.SelectMany(s => s.Modules).ToList();

    /// <summary>Looks up a section by its kebab-case section code, or null when not found.</summary>
    /// <param name="sectionCode">The section code (e.g., "core-operational").</param>
    public static ModuleSection? GetSection(string sectionCode) =>
        Sections.FirstOrDefault(s => s.SectionCode == sectionCode);

    /// <summary>Looks up a module by its route href (e.g., "/modules/hazmat"), or null when not found.</summary>
    /// <param name="href">The module route href.</param>
    public static ModuleRoute? GetModuleByHref(string href) =>
        AllModules.FirstOrDefault(m => m.Href == href);

    private static IReadOnlyList<ModuleSection> BuildSections() =>
    [
        new ModuleSection
        {
            SectionCode = "core-operational",
            Title = "Core Operational",
            Description = "Alerts, workforce, assets, and day-to-day safety operations.",
            IconClass = "bi-grid-1x2-fill",
            SubMenuHref = RouteDefinitions.MenuCoreOperational,
            Modules =
            [
                new ModuleRoute { ModuleCode = "alerts", Label = "Alerts", Href = RouteDefinitions.AlertsList, IconClass = "bi-bell", Section = "Core Operational", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "auditing", Label = "Auditing", Href = RouteDefinitions.AuditingList, IconClass = "bi-clipboard-check", Section = "Core Operational", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "worker-management", Label = "Worker Management", Href = RouteDefinitions.WorkerManagementList, IconClass = "bi-people", Section = "Core Operational", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "machine-management", Label = "Machine Management", Href = RouteDefinitions.MachineManagementList, IconClass = "bi-gear", Section = "Core Operational", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "process-management", Label = "Process Management", Href = RouteDefinitions.ProcessManagementList, IconClass = "bi-diagram-3", Section = "Core Operational", SectionOrder = 5 },
                new ModuleRoute { ModuleCode = "training", Label = "Training", Href = RouteDefinitions.TrainingList, IconClass = "bi-mortarboard", Section = "Core Operational", SectionOrder = 6 },
                new ModuleRoute { ModuleCode = "certifications", Label = "Certifications", Href = RouteDefinitions.CertificationsList, IconClass = "bi-award", Section = "Core Operational", SectionOrder = 7 },
                new ModuleRoute { ModuleCode = "ppe", Label = "PPE", Href = RouteDefinitions.PpeList, IconClass = "bi-shield-check", Section = "Core Operational", SectionOrder = 8 },
                new ModuleRoute { ModuleCode = "culture", Label = "Culture", Href = RouteDefinitions.CultureDashboard, IconClass = "bi-heart-pulse", Section = "Core Operational", SectionOrder = 9 },
                new ModuleRoute { ModuleCode = "scheduling", Label = "Scheduling", Href = RouteDefinitions.SchedulingList, IconClass = "bi-calendar3", Section = "Core Operational", SectionOrder = 10 },
                new ModuleRoute { ModuleCode = "maintenance", Label = "Maintenance", Href = RouteDefinitions.MaintenanceList, IconClass = "bi-tools", Section = "Core Operational", SectionOrder = 11 },
                new ModuleRoute { ModuleCode = "predictive-maintenance", Label = "Predictive Maintenance", Href = RouteDefinitions.PredictiveMaintenanceDashboard, IconClass = "bi-graph-up", Section = "Core Operational", SectionOrder = 12 },
                new ModuleRoute { ModuleCode = "production-tracking", Label = "Production Tracking", Href = RouteDefinitions.ProductionTrackingDashboard, IconClass = "bi-bar-chart", Section = "Core Operational", SectionOrder = 13 },
                new ModuleRoute { ModuleCode = "routing", Label = "Routing", Href = RouteDefinitions.RoutingList, IconClass = "bi-signpost-split", Section = "Core Operational", SectionOrder = 14 },
                new ModuleRoute { ModuleCode = "reporting", Label = "Reporting", Href = RouteDefinitions.ReportingList, IconClass = "bi-file-earmark-text", Section = "Core Operational", SectionOrder = 15 },
                new ModuleRoute { ModuleCode = "safety-programs", Label = "Safety Programs", Href = RouteDefinitions.SafetyProgramsList, IconClass = "bi-shield", Section = "Core Operational", SectionOrder = 16 },
                new ModuleRoute { ModuleCode = "incident-management", Label = "Incident Management", Href = RouteDefinitions.IncidentManagementList, IconClass = "bi-exclamation-triangle", Section = "Core Operational", SectionOrder = 17 },
                new ModuleRoute { ModuleCode = "document-management", Label = "Document Management", Href = RouteDefinitions.DocumentManagementList, IconClass = "bi-folder", Section = "Core Operational", SectionOrder = 18 },
                new ModuleRoute { ModuleCode = "workflow", Label = "Workflow Approvals", Href = RouteDefinitions.WorkflowApprovalsInbox, IconClass = "bi-diagram-2", Section = "Core Operational", SectionOrder = 19 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "safety-event",
            Title = "Safety Event",
            Description = "Crisis response, drills, emergencies, and event investigation.",
            IconClass = "bi-exclamation-diamond-fill",
            SubMenuHref = RouteDefinitions.MenuSafetyEvent,
            Modules =
            [
                new ModuleRoute { ModuleCode = "crisis-management", Label = "Crisis Management", Href = RouteDefinitions.CrisisManagementDashboard, IconClass = "bi-lightning", Section = "Safety Event", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "drills", Label = "Drills & Exercises", Href = RouteDefinitions.DrillsList, IconClass = "bi-stopwatch", Section = "Safety Event", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "emergency-management", Label = "Emergency Management", Href = RouteDefinitions.EmergencyManagementDashboard, IconClass = "bi-alarm", Section = "Safety Event", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "near-miss", Label = "Near Miss", Href = RouteDefinitions.NearMissList, IconClass = "bi-eye-slash", Section = "Safety Event", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "observations", Label = "Observation", Href = RouteDefinitions.ObservationList, IconClass = "bi-binoculars", Section = "Safety Event", SectionOrder = 5 },
                new ModuleRoute { ModuleCode = "rca", Label = "RCA", Href = RouteDefinitions.RcaList, IconClass = "bi-search", Section = "Safety Event", SectionOrder = 6 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "human-factors",
            Title = "Human Factors",
            Description = "Fatigue, ergonomics, industrial hygiene, and worker wellbeing.",
            IconClass = "bi-person-lines-fill",
            SubMenuHref = RouteDefinitions.MenuHumanFactors,
            Modules =
            [
                new ModuleRoute { ModuleCode = "human-factors", Label = "Human Factors", Href = RouteDefinitions.HumanFactorsDashboard, IconClass = "bi-person-lines-fill", Section = "Human Factors", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "fatigue-management", Label = "Fatigue Management", Href = RouteDefinitions.FatigueManagementDashboard, IconClass = "bi-moon", Section = "Human Factors", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "ergonomics", Label = "Ergonomics", Href = RouteDefinitions.ErgonomicsList, IconClass = "bi-body-text", Section = "Human Factors", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "industrial-hygiene", Label = "Industrial Hygiene", Href = RouteDefinitions.IndustrialHygieneList, IconClass = "bi-droplet", Section = "Human Factors", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "worker-wellbeing", Label = "Worker WellBeing", Href = RouteDefinitions.WorkerWellBeingDashboard, IconClass = "bi-heart", Section = "Human Factors", SectionOrder = 5 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "operational-safety",
            Title = "Operational Safety",
            Description = "Hazard controls, permits, LOTO, and equipment safety.",
            IconClass = "bi-shield-exclamation",
            SubMenuHref = RouteDefinitions.MenuOperationalSafety,
            Modules =
            [
                new ModuleRoute { ModuleCode = "equipment-safety", Label = "Equipment Safety", Href = RouteDefinitions.EquipmentSafetyList, IconClass = "bi-hammer", Section = "Operational Safety", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "hazard-assessment", Label = "Hazard Assessment", Href = RouteDefinitions.HazardAssessmentList, IconClass = "bi-radioactive", Section = "Operational Safety", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "hazmat", Label = "HazMat", Href = RouteDefinitions.HazMatList, IconClass = "bi-exclamation-octagon", Section = "Operational Safety", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "inspections", Label = "Inspections", Href = RouteDefinitions.InspectionsList, IconClass = "bi-clipboard2-check", Section = "Operational Safety", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "jsa", Label = "JSA", Href = RouteDefinitions.JsaList, IconClass = "bi-card-checklist", Section = "Operational Safety", SectionOrder = 5 },
                new ModuleRoute { ModuleCode = "loto", Label = "LOTO", Href = RouteDefinitions.LotoList, IconClass = "bi-lock", Section = "Operational Safety", SectionOrder = 6 },
                new ModuleRoute { ModuleCode = "permits", Label = "Permits to Work", Href = RouteDefinitions.PermitsList, IconClass = "bi-patch-check", Section = "Operational Safety", SectionOrder = 7 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "analytics",
            Title = "Analytics",
            Description = "Cross-module analytics, scorecards, and trend analysis.",
            IconClass = "bi-graph-up-arrow",
            SubMenuHref = RouteDefinitions.MenuAnalytics,
            Modules =
            [
                new ModuleRoute { ModuleCode = "culture-analytics", Label = "Culture Analytics", Href = RouteDefinitions.CultureAnalyticsDashboard, IconClass = "bi-graph-up-arrow", Section = "Analytics", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "dashboard-analytics", Label = "Dashboard Analytics", Href = RouteDefinitions.DashboardAnalytics, IconClass = "bi-speedometer2", Section = "Analytics", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "engagement-analytics", Label = "Engagement Analytics", Href = RouteDefinitions.EngagementAnalyticsDashboard, IconClass = "bi-people-fill", Section = "Analytics", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "predictive-analytics", Label = "Predictive Analytics", Href = RouteDefinitions.PredictiveAnalyticsDashboard, IconClass = "bi-cpu", Section = "Analytics", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "reporting-analytics", Label = "Reporting Analytics", Href = RouteDefinitions.ReportingAnalyticsList, IconClass = "bi-file-bar-graph", Section = "Analytics", SectionOrder = 5 },
                new ModuleRoute { ModuleCode = "scheduling-analytics", Label = "Scheduling Analytics", Href = RouteDefinitions.SchedulingAnalyticsDashboard, IconClass = "bi-calendar-check", Section = "Analytics", SectionOrder = 6 },
                new ModuleRoute { ModuleCode = "scorecards", Label = "Scorecards", Href = RouteDefinitions.ScorecardsDashboard, IconClass = "bi-trophy", Section = "Analytics", SectionOrder = 7 },
                new ModuleRoute { ModuleCode = "trend-analysis", Label = "Trend Analysis", Href = RouteDefinitions.TrendAnalysisDashboard, IconClass = "bi-activity", Section = "Analytics", SectionOrder = 8 },
                new ModuleRoute { ModuleCode = "bi-dashboards", Label = "BI Dashboards", Href = RouteDefinitions.BiDashboards, IconClass = "bi-bar-chart-line", Section = "Analytics", SectionOrder = 9 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "compliance",
            Title = "Compliance",
            Description = "Regulatory compliance, management of change, and policies.",
            IconClass = "bi-check2-all",
            SubMenuHref = RouteDefinitions.MenuCompliance,
            Modules =
            [
                new ModuleRoute { ModuleCode = "compliance", Label = "Compliance", Href = RouteDefinitions.ComplianceDashboard, IconClass = "bi-check2-all", Section = "Compliance", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "moc", Label = "MOC", Href = RouteDefinitions.MocList, IconClass = "bi-arrow-repeat", Section = "Compliance", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "policies", Label = "Policies & Procedures", Href = RouteDefinitions.PoliciesList, IconClass = "bi-journal-text", Section = "Compliance", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "regulatory-calendar", Label = "Regulatory Calendar", Href = RouteDefinitions.RegulatoryCalendar, IconClass = "bi-calendar-event", Section = "Compliance", SectionOrder = 4 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "financial",
            Title = "Financial",
            Description = "Financial tracking, ROI, checklists, and contractor management.",
            IconClass = "bi-cash-stack",
            SubMenuHref = RouteDefinitions.MenuFinancial,
            Modules =
            [
                new ModuleRoute { ModuleCode = "financials", Label = "Financials", Href = RouteDefinitions.FinancialsDashboard, IconClass = "bi-currency-dollar", Section = "Financial", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "roi", Label = "ROI", Href = RouteDefinitions.RoiDashboard, IconClass = "bi-graph-up", Section = "Financial", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "checklists", Label = "Checklist", Href = RouteDefinitions.ChecklistList, IconClass = "bi-list-check", Section = "Financial", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "contractor-management", Label = "Contractor Management", Href = RouteDefinitions.ContractorManagementList, IconClass = "bi-briefcase", Section = "Financial", SectionOrder = 4 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "intelligence",
            Title = "Intelligence",
            Description = "AI, data lake, digital twin, and enterprise fabric capabilities.",
            IconClass = "bi-cpu-fill",
            SubMenuHref = RouteDefinitions.MenuIntelligence,
            Modules =
            [
                new ModuleRoute { ModuleCode = "ai-training", Label = "AI Training", Href = RouteDefinitions.AiTrainingDashboard, IconClass = "bi-robot", Section = "Intelligence", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "ai-investigation", Label = "AI Investigation", Href = RouteDefinitions.AiInvestigationList, IconClass = "bi-search-heart", Section = "Intelligence", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "data-lake", Label = "Data Lake", Href = RouteDefinitions.DataLakeExplorer, IconClass = "bi-database", Section = "Intelligence", SectionOrder = 3 },
                new ModuleRoute { ModuleCode = "predictive-safety", Label = "Predictive Safety", Href = RouteDefinitions.PredictiveSafetyDashboard, IconClass = "bi-shield-fill-check", Section = "Intelligence", SectionOrder = 4 },
                new ModuleRoute { ModuleCode = "exports", Label = "Exports", Href = RouteDefinitions.ExportsList, IconClass = "bi-box-arrow-up", Section = "Intelligence", SectionOrder = 5 },
                new ModuleRoute { ModuleCode = "governance", Label = "Governance & Access Control", Href = RouteDefinitions.GovernanceDashboard, IconClass = "bi-key", Section = "Intelligence", SectionOrder = 6 },
                new ModuleRoute { ModuleCode = "digital-twin", Label = "Digital Twin", Href = RouteDefinitions.DigitalTwinViewer, IconClass = "bi-layers", Section = "Intelligence", SectionOrder = 7 },
                new ModuleRoute { ModuleCode = "sustainability", Label = "Sustainability", Href = RouteDefinitions.SustainabilityDashboard, IconClass = "bi-tree", Section = "Intelligence", SectionOrder = 8 },
                new ModuleRoute { ModuleCode = "ai-governance", Label = "AI Governance", Href = RouteDefinitions.AiGovernanceDashboard, IconClass = "bi-shield-lock", Section = "Intelligence", SectionOrder = 9 },
                new ModuleRoute { ModuleCode = "enterprise-fabric", Label = "Enterprise Fabric", Href = RouteDefinitions.EnterpriseFabricDashboard, IconClass = "bi-hdd-network", Section = "Intelligence", SectionOrder = 10 },
                new ModuleRoute { ModuleCode = "environmental-health", Label = "Environmental Health", Href = RouteDefinitions.EnvironmentalHealthDashboard, IconClass = "bi-flower1", Section = "Intelligence", SectionOrder = 11 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "planning-projects",
            Title = "Planning Projects",
            Description = "Project planning, budgeting, and project scheduling.",
            IconClass = "bi-kanban-fill",
            SubMenuHref = RouteDefinitions.MenuPlanningProjects,
            Modules =
            [
                new ModuleRoute { ModuleCode = "planning-projects", Label = "Planning Projects", Href = RouteDefinitions.PlanningProjectsList, IconClass = "bi-kanban", Section = "Planning Projects", SectionOrder = 1 },
                new ModuleRoute { ModuleCode = "project-budgeting", Label = "Project Budgeting", Href = RouteDefinitions.ProjectBudgetingDashboard, IconClass = "bi-wallet2", Section = "Planning Projects", SectionOrder = 2 },
                new ModuleRoute { ModuleCode = "project-scheduling", Label = "Project Scheduling", Href = RouteDefinitions.ProjectSchedulingDashboard, IconClass = "bi-calendar-range", Section = "Planning Projects", SectionOrder = 3 },
            ]
        },
        new ModuleSection
        {
            SectionCode = "platform-tools",
            Title = "Platform Tools",
            Description = "Diagnostics and platform-level tooling (not one of the 66 business modules).",
            IconClass = "bi-bug",
            SubMenuHref = null,
            Modules =
            [
                new ModuleRoute { ModuleCode = "diagnostics", Label = "Diagnostics Viewer", Href = RouteDefinitions.DiagnosticsViewer, IconClass = "bi-bug", Section = "Platform Tools", SectionOrder = 1 },
            ]
        },
    ];
}
