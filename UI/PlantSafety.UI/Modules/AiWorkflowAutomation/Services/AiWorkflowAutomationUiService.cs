// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiWorkflowAutomation.Services
//  File:           AiWorkflowAutomationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's WorkflowAutomationController (AI.WorkflowAutomation, Phase E.3).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiWorkflowAutomation.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Workflow Automation console endpoint.</summary>
public sealed class AiWorkflowAutomationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Generates workflow automation artifacts from the given input JSON.</summary>
    public Task<string> AutomateAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/workflow-automation/automate", requestJson, cancellationToken);
}
