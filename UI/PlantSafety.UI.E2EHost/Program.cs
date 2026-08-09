// ============================================================================
//  Project:        PlantSafety.UI.E2EHost
//  File:           Program.cs
//  Created:        2026-08-09
//  Description:    Composition root. Mirrors PlantSafety.UI's MauiProgram
//                  DI registrations exactly (same UiService/State types,
//                  same PlantSafetyApiClient wrapper) so the pages under
//                  test run against production wiring, not test doubles.
// ============================================================================
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

string apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5129/";
builder.Services.AddHttpClient<PlantSafetyApiClient>(client => client.BaseAddress = new Uri(apiBaseAddress));

builder.Services.AddScoped<FacilityUiService>();
builder.Services.AddScoped<FacilityListState>();
builder.Services.AddScoped<FacilityDetailState>();

builder.Services.AddScoped<EmployeeUiService>();
builder.Services.AddScoped<EmployeeListState>();
builder.Services.AddScoped<EmployeeDetailState>();

builder.Services.AddScoped<MachineUiService>();
builder.Services.AddScoped<MachineListState>();
builder.Services.AddScoped<MachineDetailState>();

builder.Services.AddScoped<WorkflowEngineUiService>();
builder.Services.AddScoped<WorkflowEngineConsoleState>();

builder.Services.AddScoped<PredictiveModelsUiService>();
builder.Services.AddScoped<PredictiveModelsConsoleState>();

builder.Services.AddScoped<DataLakeUiService>();
builder.Services.AddScoped<DataLakeDashboardState>();

builder.Services.AddScoped<EnterpriseGraphUiService>();
builder.Services.AddScoped<EnterpriseGraphConsoleState>();

builder.Services.AddScoped<EnterpriseDashboardUiService>();
builder.Services.AddScoped<EnterpriseDashboardState>();

builder.Services.AddScoped<RulesEngineUiService>();
builder.Services.AddScoped<RulesEngineConsoleState>();

builder.Services.AddScoped<WorkflowOrchestrationUiService>();
builder.Services.AddScoped<WorkflowOrchestrationConsoleState>();

builder.Services.AddScoped<DigitalTwinUiService>();
builder.Services.AddScoped<DigitalTwinConsoleState>();

builder.Services.AddScoped<PredictiveModelingUiService>();
builder.Services.AddScoped<PredictiveModelingConsoleState>();

builder.Services.AddScoped<IntegrationUiService>();
builder.Services.AddScoped<IntegrationConsoleState>();

builder.Services.AddScoped<TenantFacilityRegistryUiService>();
builder.Services.AddScoped<TenantFacilityRegistryListState>();

builder.Services.AddScoped<ComplianceRiskUiService>();
builder.Services.AddScoped<ComplianceRiskAssessState>();

builder.Services.AddScoped<AiReasoningUiService>();
builder.Services.AddScoped<AiReasoningConsoleState>();

builder.Services.AddScoped<AiHazardUnderstandingUiService>();
builder.Services.AddScoped<AiHazardUnderstandingConsoleState>();

builder.Services.AddScoped<AiWorkflowAutomationUiService>();
builder.Services.AddScoped<AiWorkflowAutomationConsoleState>();

builder.Services.AddScoped<AiRegulatoryInterpretationUiService>();
builder.Services.AddScoped<AiRegulatoryInterpretationConsoleState>();

builder.Services.AddScoped<AiSeverityAssessmentUiService>();
builder.Services.AddScoped<AiSeverityAssessmentConsoleState>();

builder.Services.AddScoped<AiPredictiveOptimizationUiService>();
builder.Services.AddScoped<AiPredictiveOptimizationConsoleState>();

builder.Services.AddScoped<AiDigitalTwinLearningUiService>();
builder.Services.AddScoped<AiDigitalTwinLearningConsoleState>();

builder.Services.AddScoped<AiSafetyCopilotUiService>();
builder.Services.AddScoped<AiSafetyCopilotDashboardState>();

builder.Services.AddScoped<AiEnterpriseOrchestrationUiService>();
builder.Services.AddScoped<AiEnterpriseOrchestrationConsoleState>();

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

/// <summary>Marker partial for WebApplicationFactory-based test hosting.</summary>
public partial class Program;
