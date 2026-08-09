// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Host
//  File:           MauiProgram.cs
//  Created:        2026-08-09
//  Description:    Composition root. Explicit DI registration only -- no
//                  dynamic service registration, no reflection-based
//                  discovery (Section 3-4). Registers the one approved
//                  HttpClient/PlantSafetyApiClient wrapper (base address ->
//                  PlantSafety.UI.Api's local-dev http profile) plus every
//                  module's UI service and state container.
// ============================================================================
using Microsoft.Extensions.Logging;

using PlantSafety.UI.Services;

using PlantSafety.UI.Modules.Facility.Services;
using PlantSafety.UI.Modules.Facility.State;
using PlantSafety.UI.Modules.Employee.Services;
using PlantSafety.UI.Modules.Employee.State;
using PlantSafety.UI.Modules.Machine.Services;
using PlantSafety.UI.Modules.Machine.State;
using PlantSafety.UI.Modules.WorkflowEngine.Services;
using PlantSafety.UI.Modules.WorkflowEngine.State;
using PlantSafety.UI.Modules.PredictiveModels.Services;
using PlantSafety.UI.Modules.PredictiveModels.State;
using PlantSafety.UI.Modules.DataLake.Services;
using PlantSafety.UI.Modules.DataLake.State;
using PlantSafety.UI.Modules.EnterpriseGraph.Services;
using PlantSafety.UI.Modules.EnterpriseGraph.State;
using PlantSafety.UI.Modules.EnterpriseDashboard.Services;
using PlantSafety.UI.Modules.EnterpriseDashboard.State;
using PlantSafety.UI.Modules.RulesEngine.Services;
using PlantSafety.UI.Modules.RulesEngine.State;
using PlantSafety.UI.Modules.WorkflowOrchestration.Services;
using PlantSafety.UI.Modules.WorkflowOrchestration.State;
using PlantSafety.UI.Modules.DigitalTwin.Services;
using PlantSafety.UI.Modules.DigitalTwin.State;
using PlantSafety.UI.Modules.PredictiveModeling.Services;
using PlantSafety.UI.Modules.PredictiveModeling.State;
using PlantSafety.UI.Modules.Integration.Services;
using PlantSafety.UI.Modules.Integration.State;
using PlantSafety.UI.Modules.TenantFacilityRegistry.Services;
using PlantSafety.UI.Modules.TenantFacilityRegistry.State;
using PlantSafety.UI.Modules.ComplianceRisk.Services;
using PlantSafety.UI.Modules.ComplianceRisk.State;
using PlantSafety.UI.Modules.AiReasoning.Services;
using PlantSafety.UI.Modules.AiReasoning.State;
using PlantSafety.UI.Modules.AiHazardUnderstanding.Services;
using PlantSafety.UI.Modules.AiHazardUnderstanding.State;
using PlantSafety.UI.Modules.AiWorkflowAutomation.Services;
using PlantSafety.UI.Modules.AiWorkflowAutomation.State;
using PlantSafety.UI.Modules.AiRegulatoryInterpretation.Services;
using PlantSafety.UI.Modules.AiRegulatoryInterpretation.State;
using PlantSafety.UI.Modules.AiSeverityAssessment.Services;
using PlantSafety.UI.Modules.AiSeverityAssessment.State;
using PlantSafety.UI.Modules.AiPredictiveOptimization.Services;
using PlantSafety.UI.Modules.AiPredictiveOptimization.State;
using PlantSafety.UI.Modules.AiDigitalTwinLearning.Services;
using PlantSafety.UI.Modules.AiDigitalTwinLearning.State;
using PlantSafety.UI.Modules.AiSafetyCopilot.Services;
using PlantSafety.UI.Modules.AiSafetyCopilot.State;
using PlantSafety.UI.Modules.AiEnterpriseOrchestration.Services;
using PlantSafety.UI.Modules.AiEnterpriseOrchestration.State;

namespace PlantSafety.UI;

public static class MauiProgram
{
    /// <summary>Local-dev base address for PlantSafety.UI.Api's http profile (see its launchSettings.json).</summary>
    private const string ApiBaseAddress = "http://localhost:5129/";

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

        // ---- The one approved typed HttpClient wrapper (Section 10) ----
        builder.Services.AddHttpClient<PlantSafetyApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseAddress);
        });

        // ---- Facility module ----
        builder.Services.AddScoped<FacilityUiService>();
        builder.Services.AddScoped<FacilityListState>();
        builder.Services.AddScoped<FacilityDetailState>();

        // ---- Employee module ----
        builder.Services.AddScoped<EmployeeUiService>();
        builder.Services.AddScoped<EmployeeListState>();
        builder.Services.AddScoped<EmployeeDetailState>();

        // ---- Machine module ----
        builder.Services.AddScoped<MachineUiService>();
        builder.Services.AddScoped<MachineListState>();
        builder.Services.AddScoped<MachineDetailState>();

        // ---- Workflow Engine module ----
        builder.Services.AddScoped<WorkflowEngineUiService>();
        builder.Services.AddScoped<WorkflowEngineConsoleState>();

        // ---- Predictive Models module ----
        builder.Services.AddScoped<PredictiveModelsUiService>();
        builder.Services.AddScoped<PredictiveModelsConsoleState>();

        // ---- Data Lake module ----
        builder.Services.AddScoped<DataLakeUiService>();
        builder.Services.AddScoped<DataLakeDashboardState>();

        // ---- Enterprise Graph module ----
        builder.Services.AddScoped<EnterpriseGraphUiService>();
        builder.Services.AddScoped<EnterpriseGraphConsoleState>();

        // ---- Enterprise Dashboard module ----
        builder.Services.AddScoped<EnterpriseDashboardUiService>();
        builder.Services.AddScoped<EnterpriseDashboardState>();

        // ---- Rules Engine module ----
        builder.Services.AddScoped<RulesEngineUiService>();
        builder.Services.AddScoped<RulesEngineConsoleState>();

        // ---- Workflow Orchestration module ----
        builder.Services.AddScoped<WorkflowOrchestrationUiService>();
        builder.Services.AddScoped<WorkflowOrchestrationConsoleState>();

        // ---- Digital Twin module ----
        builder.Services.AddScoped<DigitalTwinUiService>();
        builder.Services.AddScoped<DigitalTwinConsoleState>();

        // ---- Predictive Modeling module ----
        builder.Services.AddScoped<PredictiveModelingUiService>();
        builder.Services.AddScoped<PredictiveModelingConsoleState>();

        // ---- Integration module ----
        builder.Services.AddScoped<IntegrationUiService>();
        builder.Services.AddScoped<IntegrationConsoleState>();

        // ---- Tenant Facility Registry module ----
        builder.Services.AddScoped<TenantFacilityRegistryUiService>();
        builder.Services.AddScoped<TenantFacilityRegistryListState>();

        // ---- Compliance & Risk module ----
        builder.Services.AddScoped<ComplianceRiskUiService>();
        builder.Services.AddScoped<ComplianceRiskAssessState>();

        // ---- AI Reasoning module ----
        builder.Services.AddScoped<AiReasoningUiService>();
        builder.Services.AddScoped<AiReasoningConsoleState>();

        // ---- AI Hazard Understanding module ----
        builder.Services.AddScoped<AiHazardUnderstandingUiService>();
        builder.Services.AddScoped<AiHazardUnderstandingConsoleState>();

        // ---- AI Workflow Automation module ----
        builder.Services.AddScoped<AiWorkflowAutomationUiService>();
        builder.Services.AddScoped<AiWorkflowAutomationConsoleState>();

        // ---- AI Regulatory Interpretation module ----
        builder.Services.AddScoped<AiRegulatoryInterpretationUiService>();
        builder.Services.AddScoped<AiRegulatoryInterpretationConsoleState>();

        // ---- AI Severity Assessment module ----
        builder.Services.AddScoped<AiSeverityAssessmentUiService>();
        builder.Services.AddScoped<AiSeverityAssessmentConsoleState>();

        // ---- AI Predictive Optimization module ----
        builder.Services.AddScoped<AiPredictiveOptimizationUiService>();
        builder.Services.AddScoped<AiPredictiveOptimizationConsoleState>();

        // ---- AI Digital Twin Learning module ----
        builder.Services.AddScoped<AiDigitalTwinLearningUiService>();
        builder.Services.AddScoped<AiDigitalTwinLearningConsoleState>();

        // ---- AI Safety Copilot module ----
        builder.Services.AddScoped<AiSafetyCopilotUiService>();
        builder.Services.AddScoped<AiSafetyCopilotDashboardState>();

        // ---- AI Enterprise Orchestration module ----
        builder.Services.AddScoped<AiEnterpriseOrchestrationUiService>();
        builder.Services.AddScoped<AiEnterpriseOrchestrationConsoleState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
