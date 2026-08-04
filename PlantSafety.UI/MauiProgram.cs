// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: MAUI application composition root. Registers all
//   typed HttpClients (with correlation and auth handler pipelines),
//   singleton state containers, MSAL token provider, and structured
//   logging. All services MUST be registered here; no ambient service
//   location is permitted in components or state containers.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.6, §XI
// ============================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

using PlantSafety.UI.Infrastructure.Auth;
using PlantSafety.UI.Infrastructure.Correlation;
using PlantSafety.UI.Modules.Alerts.ApiClients;
using PlantSafety.UI.Modules.BiDashboards.ApiClients;
using PlantSafety.UI.Modules.DiagnosticsViewer.Services;
using PlantSafety.UI.Modules.OperationsAuditViewer.ApiClients;
using PlantSafety.UI.Modules.OperationsBackup.ApiClients;
using PlantSafety.UI.Modules.SchemaGuard.ApiClients;
using PlantSafety.UI.Modules.OperationsArchival.ApiClients;
using PlantSafety.UI.Modules.OperationsMonitoring.ApiClients;
using PlantSafety.UI.Modules.OperationsConfiguration.ApiClients;
using PlantSafety.UI.Modules.OperationsSystemHealth.ApiClients;
using PlantSafety.UI.Modules.Workflow.ApiClients;
using PlantSafety.UI.Modules.Auditing.ApiClients;
using PlantSafety.UI.Modules.Compliance.ApiClients;
using PlantSafety.UI.State;

namespace PlantSafety.UI;

/// <summary>
/// MAUI application entry point and DI composition root.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds and returns the configured <see cref="MauiApp"/> instance.
    /// All registrations follow the dependency matrix defined in §PART XXI.
    /// </summary>
    /// <returns>The fully configured MauiApp ready to launch.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // ── Configuration ──────────────────────────────────────────────────────
        // appsettings.json is embedded as a MAUI raw asset; loaded via EmbeddedResource.
        // AzureAd and Api sections are required; missing keys throw at startup.

        // ── Correlation ────────────────────────────────────────────────────────
        // Scoped so each Blazor circuit/session gets its own lineage envelope.
        builder.Services.AddScoped<CorrelationContext>();
        builder.Services.AddTransient<CorrelationHandler>();

        // ── Authentication ─────────────────────────────────────────────────────
        builder.Services.AddSingleton<IMsalTokenProvider, MsalTokenProvider>();
        builder.Services.AddTransient<AuthorizationMessageHandler>();

        // ── Impersonation (Phase 7 §6.3 Support Tooling) ────────────────────────
        // Stamps X-Tenant-ID on every outbound request while a support-console
        // impersonation session is active; registered in every typed client's
        // handler pipeline via RegisterApiClient below.
        builder.Services.AddTransient<Infrastructure.Impersonation.ImpersonationHeaderHandler>();

        // ── State Containers (Singleton — shared across the session) ───────────
        builder.Services.AddSingleton<AppSessionState>();
        builder.Services.AddSingleton<UserSessionState>();
        builder.Services.AddSingleton<TenantState>();
        builder.Services.AddSingleton<NavigationState>();
        builder.Services.AddSingleton<Modules.Admin.State.TenantAdminState>();
        builder.Services.AddSingleton<Modules.Admin.State.BillingState>();
        builder.Services.AddSingleton<Modules.Admin.State.ImpersonationState>();
        builder.Services.AddSingleton<Modules.Admin.State.SupportConsoleState>();
        builder.Services.AddSingleton<Modules.Admin.State.UserManagementState>();
        builder.Services.AddSingleton<Modules.Admin.State.TenantFeatureFlagsState>();
        builder.Services.AddSingleton<Modules.Admin.State.GlobalSafetyDashboardState>();

        // ── Module State Containers ────────────────────────────────────────────
        builder.Services.AddSingleton<Modules.Alerts.State.AlertsState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.DiagnosticsSessionState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.FilterState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.SortState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.SelectedResultState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.ExportState>();
        builder.Services.AddSingleton<Modules.DiagnosticsViewer.State.ComparisonState>();
        builder.Services.AddSingleton<Modules.ProcessManagement.State.ProcessManagementState>();
        builder.Services.AddSingleton<Modules.WorkerWellBeing.State.WorkerWellBeingState>();
        builder.Services.AddSingleton<Modules.EnvironmentalHealth.State.EnvironmentalHealthState>();
        builder.Services.AddSingleton<Modules.OperationsBackup.State.BackupState>();
        builder.Services.AddSingleton<Modules.SchemaGuard.State.SchemaGuardState>();
        builder.Services.AddSingleton<Modules.OperationsMonitoring.State.MonitoringState>();
        builder.Services.AddSingleton<Modules.OperationsArchival.State.ArchivalState>();
        builder.Services.AddSingleton<Modules.OperationsAuditViewer.State.AuditViewerState>();
        builder.Services.AddSingleton<Modules.OperationsConfiguration.State.ConfigurationState>();
        builder.Services.AddSingleton<Modules.OperationsSystemHealth.State.SystemHealthState>();
        builder.Services.AddSingleton<Modules.BiDashboards.State.BiDashboardsState>();
        builder.Services.AddSingleton<Modules.Workflow.State.WorkflowState>();
        builder.Services.AddSingleton<Modules.Auditing.State.AuditingState>();
        builder.Services.AddSingleton<Modules.Compliance.State.ComplianceState>();

        // Regression audit fix (2026-08-03): 84 module State containers existed
        // fully-implemented but were never registered here, so any page injecting one of them
        // would crash with a DI resolution failure the moment a user navigated to it. Closes the
        // gap, following the exact AddSingleton<Modules.X.State.Y> pattern established above.
        builder.Services.AddSingleton<Modules.Admin.State.AdminSetupState>();
        builder.Services.AddSingleton<Modules.AiGovernance.State.AiGovernanceState>();
        builder.Services.AddSingleton<Modules.AiInvestigation.State.AiInvestigationState>();
        builder.Services.AddSingleton<Modules.AiTraining.State.AiTrainingState>();
        builder.Services.AddSingleton<Modules.Certifications.State.CertificationsState>();
        builder.Services.AddSingleton<Modules.Checklist.State.ChecklistState>();
        builder.Services.AddSingleton<Modules.ContractorManagement.State.ContractorManagementState>();
        builder.Services.AddSingleton<Modules.CrisisManagement.State.CrisisManagementState>();
        builder.Services.AddSingleton<Modules.Culture.State.CultureState>();
        builder.Services.AddSingleton<Modules.CultureAnalytics.State.CultureAnalyticsState>();
        builder.Services.AddSingleton<Modules.DashboardAnalytics.State.DashboardAnalyticsState>();
        builder.Services.AddSingleton<Modules.DataLake.State.DataLakeState>();
        builder.Services.AddSingleton<Modules.DigitalTwin.State.DigitalTwinState>();
        builder.Services.AddSingleton<Modules.DocumentManagement.State.DocumentManagementState>();
        builder.Services.AddSingleton<Modules.Drills.State.DrillsState>();
        builder.Services.AddSingleton<Modules.EmergencyManagement.State.EmergencyManagementState>();
        builder.Services.AddSingleton<Modules.EngagementAnalytics.State.EngagementAnalyticsState>();
        builder.Services.AddSingleton<Modules.EnterpriseFabric.State.EnterpriseFabricState>();
        builder.Services.AddSingleton<Modules.EquipmentSafety.State.EquipmentSafetyState>();
        builder.Services.AddSingleton<Modules.Ergonomics.State.ErgonomicsState>();
        builder.Services.AddSingleton<Modules.Exports.State.ExportsState>();
        builder.Services.AddSingleton<Modules.FatigueManagement.State.FatigueManagementState>();
        builder.Services.AddSingleton<Modules.Financials.State.FinancialsState>();
        builder.Services.AddSingleton<Modules.Governance.State.GovernanceState>();
        builder.Services.AddSingleton<Modules.HazMat.State.HazMatState>();
        builder.Services.AddSingleton<Modules.HazardAssessment.State.HazardAssessmentState>();
        builder.Services.AddSingleton<Modules.HumanFactors.State.HumanFactorsState>();
        builder.Services.AddSingleton<Modules.HumanFactors.CognitiveLoadAnalytics.State.CognitiveLoadAnalyticsState>();
        builder.Services.AddSingleton<Modules.IncidentManagement.State.IncidentDetailState>();
        builder.Services.AddSingleton<Modules.IncidentManagement.State.IncidentTimelinePageState>();
        builder.Services.AddSingleton<Modules.IncidentManagement.State.IncidentsListState>();
        builder.Services.AddSingleton<Modules.IncidentManagement.State.SlaMonitorState>();
        builder.Services.AddSingleton<Modules.IndustrialHygiene.State.IndustrialHygieneState>();
        builder.Services.AddSingleton<Modules.Inspections.State.InspectionsState>();
        builder.Services.AddSingleton<Modules.Jsa.State.JsaState>();
        builder.Services.AddSingleton<Modules.LocalEmergencyCoordination.State.LocalEmergencyCoordinationState>();
        builder.Services.AddSingleton<Modules.HazardousWasteManagement.State.HazardousWasteManagementState>();
        builder.Services.AddSingleton<Modules.Loto.State.LotoState>();
        builder.Services.AddSingleton<Modules.MOC.State.MocState>();
        builder.Services.AddSingleton<Modules.MachineManagement.State.MachineManagementState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.MachineHistoryState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.MaintenanceCalendarState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.MaintenanceDashboardState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.PartsInventoryState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.PreventiveScheduleState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.TechnicianWorkloadState>();
        builder.Services.AddSingleton<Modules.Maintenance.State.WorkOrderDetailState>();
        builder.Services.AddSingleton<Modules.NearMiss.State.NearMissState>();
        builder.Services.AddSingleton<Modules.Observations.State.ObservationState>();
        builder.Services.AddSingleton<Modules.PPE.State.PpeState>();
        builder.Services.AddSingleton<Modules.Permits.State.PermitsState>();
        builder.Services.AddSingleton<Modules.PlanningProjects.State.PlanningProjectsState>();
        builder.Services.AddSingleton<Modules.Policies.State.PoliciesState>();
        builder.Services.AddSingleton<Modules.PredictiveAnalytics.State.PredictiveAnalyticsState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.EquipmentPredictiveViewState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.PredictionDetailState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.PredictionsListState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.PredictiveDashboardState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.PredictiveReportsState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.RecommendationDetailState>();
        builder.Services.AddSingleton<Modules.PredictiveMaintenance.State.SensorExplorerState>();
        builder.Services.AddSingleton<Modules.PredictiveSafety.State.PredictiveSafetyState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.BatchTrackingState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.EventHistoryState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.KpiDashboardState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.ProductionDashboardState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.ProductionRunDetailState>();
        builder.Services.AddSingleton<Modules.ProductionTracking.State.ProductionTimelineState>();
        builder.Services.AddSingleton<Modules.ProjectBudgeting.State.ProjectBudgetingState>();
        builder.Services.AddSingleton<Modules.ProjectScheduling.State.ProjectSchedulingState>();
        builder.Services.AddSingleton<Modules.ROI.State.RoiState>();
        builder.Services.AddSingleton<Modules.Rca.State.RcaState>();
        builder.Services.AddSingleton<Modules.RegulatoryCalendar.State.RegulatoryCalendarState>();
        builder.Services.AddSingleton<Modules.Reporting.State.ReportingState>();
        builder.Services.AddSingleton<Modules.ReportingAnalytics.State.ReportingAnalyticsState>();
        builder.Services.AddSingleton<Modules.Routing.State.RoutingDashboardState>();
        builder.Services.AddSingleton<Modules.Routing.State.RoutingDetailState>();
        builder.Services.AddSingleton<Modules.SafetyPrograms.State.SafetyProgramsState>();
        builder.Services.AddSingleton<Modules.Scheduling.State.JobDetailState>();
        builder.Services.AddSingleton<Modules.Scheduling.State.SchedulingCalendarState>();
        builder.Services.AddSingleton<Modules.Scheduling.State.SchedulingDashboardState>();
        builder.Services.AddSingleton<Modules.SchedulingAnalytics.State.SchedulingAnalyticsState>();
        builder.Services.AddSingleton<Modules.Scorecards.State.ScorecardsState>();
        builder.Services.AddSingleton<Modules.Sustainability.State.SustainabilityState>();
        builder.Services.AddSingleton<Modules.Sustainability.AdvancedMetrics.State.AdvancedMetricsState>();
        builder.Services.AddSingleton<Modules.Training.State.TrainingState>();
        builder.Services.AddSingleton<Modules.TrendAnalysis.State.TrendAnalysisState>();
        builder.Services.AddSingleton<Modules.WorkerManagement.State.WorkerManagementState>();

        // ── Typed HttpClients ──────────────────────────────────────────────────
        // Handler pipeline: CorrelationHandler → AuthorizationMessageHandler →
        // ImpersonationHeaderHandler → HttpClientHandler (Phase 7 §6.3: stamps X-Tenant-ID
        // while a support-console impersonation session is active).
        // Base address is read from configuration; must end with trailing slash.
        RegisterApiClient<AlertsApiClient>(builder, "alerts");
        RegisterApiClient<DiagnosticsApiClient>(builder, "diagnostics");
        RegisterApiClient<Modules.ProcessManagement.ApiClients.ProcessManagementApiClient>(builder, "process-management");
        RegisterApiClient<Modules.WorkerWellBeing.ApiClients.WorkerWellBeingApiClient>(builder, "worker-wellbeing");
        RegisterApiClient<Modules.EnvironmentalHealth.ApiClients.EnvironmentalHealthApiClient>(builder, "environmental-health");
        RegisterApiClient<BackupApiClient>(builder, "operations-backup");
        RegisterApiClient<SchemaGuardApiClient>(builder, "operations-schema-guard");
        RegisterApiClient<MonitoringApiClient>(builder, "operations-monitoring");
        RegisterApiClient<ArchivalApiClient>(builder, "operations-archival");
        RegisterApiClient<AuditViewerApiClient>(builder, "operations-audit-viewer");
        RegisterApiClient<ConfigurationApiClient>(builder, "operations-configuration");
        RegisterApiClient<SystemHealthApiClient>(builder, "operations-system-health");
        RegisterApiClient<BiDashboardsApiClient>(builder, "bi-dashboards");
        RegisterApiClient<WorkflowApiClient>(builder, "workflow");
        RegisterApiClient<AuditingApiClient>(builder, "auditing");
        RegisterApiClient<ComplianceApiClient>(builder, "compliance");

        // Regression audit fix (2026-08-03): 56 module ApiClients existed fully-implemented but
        // were never registered as typed HttpClients here, so any page injecting one of them
        // would crash with a DI resolution failure the moment a user navigated to it. Closes the
        // gap, following the exact RegisterApiClient<T>(builder, "kebab-case-name") pattern
        // established above.
        RegisterApiClient<Modules.Admin.ApiClients.AdminApiClient>(builder, "admin");
        RegisterApiClient<Modules.Admin.ApiClients.TenantAdminApiClient>(builder, "tenant-admin");
        RegisterApiClient<Modules.Admin.ApiClients.BillingApiClient>(builder, "billing");
        RegisterApiClient<Modules.Admin.ApiClients.ImpersonationApiClient>(builder, "impersonation");
        RegisterApiClient<Modules.Admin.ApiClients.ComplianceMonitoringApiClient>(builder, "compliance-monitoring");
        RegisterApiClient<Modules.Admin.ApiClients.UserManagementApiClient>(builder, "user-management");
        RegisterApiClient<Modules.Admin.ApiClients.GlobalSafetyDashboardApiClient>(builder, "global-safety-dashboard");
        RegisterApiClient<Modules.AiGovernance.ApiClients.AiGovernanceApiClient>(builder, "ai-governance");
        RegisterApiClient<Modules.AiInvestigation.ApiClients.AiInvestigationApiClient>(builder, "ai-investigation");
        RegisterApiClient<Modules.AiTraining.ApiClients.AiTrainingApiClient>(builder, "ai-training");
        RegisterApiClient<Modules.Certifications.ApiClients.CertificationsApiClient>(builder, "certifications");
        RegisterApiClient<Modules.Checklist.ApiClients.ChecklistApiClient>(builder, "checklist");
        RegisterApiClient<Modules.ContractorManagement.ApiClients.ContractorManagementApiClient>(builder, "contractor-management");
        RegisterApiClient<Modules.CrisisManagement.ApiClients.CrisisManagementApiClient>(builder, "crisis-management");
        RegisterApiClient<Modules.CultureAnalytics.ApiClients.CultureAnalyticsApiClient>(builder, "culture-analytics");
        RegisterApiClient<Modules.Culture.ApiClients.CultureApiClient>(builder, "culture");
        RegisterApiClient<Modules.DashboardAnalytics.ApiClients.DashboardAnalyticsApiClient>(builder, "dashboard-analytics");
        RegisterApiClient<Modules.DataLake.ApiClients.DataLakeApiClient>(builder, "data-lake");
        RegisterApiClient<Modules.DigitalTwin.ApiClients.DigitalTwinApiClient>(builder, "digital-twin");
        RegisterApiClient<Modules.DocumentManagement.ApiClients.DocumentManagementApiClient>(builder, "document-management");
        RegisterApiClient<Modules.Drills.ApiClients.DrillsApiClient>(builder, "drills");
        RegisterApiClient<Modules.EmergencyManagement.ApiClients.EmergencyManagementApiClient>(builder, "emergency-management");
        RegisterApiClient<Modules.EngagementAnalytics.ApiClients.EngagementAnalyticsApiClient>(builder, "engagement-analytics");
        RegisterApiClient<Modules.EnterpriseFabric.ApiClients.EnterpriseFabricApiClient>(builder, "enterprise-fabric");
        RegisterApiClient<Modules.EquipmentSafety.ApiClients.EquipmentSafetyApiClient>(builder, "equipment-safety");
        RegisterApiClient<Modules.Ergonomics.ApiClients.ErgonomicsApiClient>(builder, "ergonomics");
        RegisterApiClient<Modules.Exports.ApiClients.ExportsApiClient>(builder, "exports");
        RegisterApiClient<Modules.FatigueManagement.ApiClients.FatigueManagementApiClient>(builder, "fatigue-management");
        RegisterApiClient<Modules.Financials.ApiClients.FinancialsApiClient>(builder, "financials");
        RegisterApiClient<Modules.HazMat.ApiClients.HazMatApiClient>(builder, "haz-mat");
        RegisterApiClient<Modules.HazardousWasteManagement.ApiClients.HazardousWasteManagementApiClient>(builder, "hazardous-waste-management");
        RegisterApiClient<Modules.HazardAssessment.ApiClients.HazardAssessmentApiClient>(builder, "hazard-assessment");
        RegisterApiClient<Modules.HumanFactors.CognitiveLoadAnalytics.ApiClients.CognitiveLoadAnalyticsApiClient>(builder, "human-factors-cognitive-load-analytics");
        RegisterApiClient<Modules.IncidentManagement.ApiClients.IncidentManagementApiClient>(builder, "incident-management");
        RegisterApiClient<Modules.IndustrialHygiene.ApiClients.IndustrialHygieneApiClient>(builder, "industrial-hygiene");
        RegisterApiClient<Modules.Inspections.ApiClients.InspectionsApiClient>(builder, "inspections");
        RegisterApiClient<Modules.Jsa.ApiClients.JsaApiClient>(builder, "jsa");
        RegisterApiClient<Modules.LocalEmergencyCoordination.ApiClients.LocalEmergencyCoordinationApiClient>(builder, "local-emergency-coordination");
        RegisterApiClient<Modules.Loto.ApiClients.LotoApiClient>(builder, "loto");
        RegisterApiClient<Modules.MachineManagement.ApiClients.MachineManagementApiClient>(builder, "machine-management");
        RegisterApiClient<Modules.Maintenance.ApiClients.MaintenanceApiClient>(builder, "maintenance");
        RegisterApiClient<Modules.MOC.ApiClients.MocApiClient>(builder, "moc");
        RegisterApiClient<Modules.NearMiss.ApiClients.NearMissApiClient>(builder, "near-miss");
        RegisterApiClient<Modules.Observations.ApiClients.ObservationApiClient>(builder, "observation");
        RegisterApiClient<Modules.Permits.ApiClients.PermitsApiClient>(builder, "permits");
        RegisterApiClient<Modules.PlanningProjects.ApiClients.PlanningProjectsApiClient>(builder, "planning-projects");
        RegisterApiClient<Modules.Policies.ApiClients.PoliciesApiClient>(builder, "policies");
        RegisterApiClient<Modules.PPE.ApiClients.PpeApiClient>(builder, "ppe");
        RegisterApiClient<Modules.PredictiveAnalytics.ApiClients.PredictiveAnalyticsApiClient>(builder, "predictive-analytics");
        RegisterApiClient<Modules.PredictiveMaintenance.ApiClients.PredictiveMaintenanceApiClient>(builder, "predictive-maintenance");
        RegisterApiClient<Modules.PredictiveSafety.ApiClients.PredictiveSafetyApiClient>(builder, "predictive-safety");
        RegisterApiClient<Modules.ProductionTracking.ApiClients.ProductionTrackingApiClient>(builder, "production-tracking");
        RegisterApiClient<Modules.Rca.ApiClients.RcaApiClient>(builder, "rca");
        RegisterApiClient<Modules.RegulatoryCalendar.ApiClients.RegulatoryCalendarApiClient>(builder, "regulatory-calendar");
        RegisterApiClient<Modules.ReportingAnalytics.ApiClients.ReportingAnalyticsApiClient>(builder, "reporting-analytics");
        RegisterApiClient<Modules.Reporting.ApiClients.ReportingApiClient>(builder, "reporting");
        RegisterApiClient<Modules.Routing.ApiClients.RoutingApiClient>(builder, "routing");
        RegisterApiClient<Modules.SafetyPrograms.ApiClients.SafetyProgramsApiClient>(builder, "safety-programs");
        RegisterApiClient<Modules.SchedulingAnalytics.ApiClients.SchedulingAnalyticsApiClient>(builder, "scheduling-analytics");
        RegisterApiClient<Modules.Scheduling.ApiClients.SchedulingApiClient>(builder, "scheduling");
        RegisterApiClient<Modules.Scorecards.ApiClients.ScorecardsApiClient>(builder, "scorecards");
        RegisterApiClient<Modules.Sustainability.ApiClients.SustainabilityApiClient>(builder, "sustainability");
        RegisterApiClient<Modules.Sustainability.AdvancedMetrics.ApiClients.AdvancedMetricsApiClient>(builder, "sustainability-advanced-metrics");
        RegisterApiClient<Modules.Training.ApiClients.TrainingApiClient>(builder, "training");
        RegisterApiClient<Modules.TrendAnalysis.ApiClients.TrendAnalysisApiClient>(builder, "trend-analysis");
        RegisterApiClient<Modules.WorkerManagement.ApiClients.WorkerManagementApiClient>(builder, "worker-management");

        // ── Logging ────────────────────────────────────────────────────────────
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary>
    /// Registers a typed API client with the canonical handler pipeline:
    /// CorrelationHandler → AuthorizationMessageHandler → ImpersonationHeaderHandler.
    /// </summary>
    /// <typeparam name="TClient">The typed client implementation.</typeparam>
    /// <param name="builder">The MAUI app builder.</param>
    /// <param name="clientName">A logical name used for diagnostics and configuration lookup.</param>
    private static void RegisterApiClient<TClient>(MauiAppBuilder builder, string clientName)
        where TClient : class
    {
        builder.Services
            .AddHttpClient<TClient>(clientName, client =>
            {
                // Base address is read from appsettings.json Api:BaseUrl at runtime.
                // Cannot inline IConfiguration here — clients resolve it from the
                // IServiceProvider after MauiApp is built.
            })
            .AddHttpMessageHandler<CorrelationHandler>()
            .AddHttpMessageHandler<AuthorizationMessageHandler>()
            .AddHttpMessageHandler<Infrastructure.Impersonation.ImpersonationHeaderHandler>();
    }
}
