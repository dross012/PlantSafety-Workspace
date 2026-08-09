// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.WorkflowEngine.Services
//  File:           WorkflowEngineUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's WorkflowEngineController.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.WorkflowEngine.Services;

/// <summary>Calls PlantSafety.UI.Api's Workflow Engine console endpoint.</summary>
public sealed class WorkflowEngineUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Evaluates the given WorkflowContext JSON and returns the formatted WorkflowDecision JSON.</summary>
    public Task<string> EvaluateAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/workflow/evaluate", requestJson, cancellationToken);
}
