// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.WorkflowOrchestration.Services
//  File:           WorkflowOrchestrationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's WorkflowOrchestrationController.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.WorkflowOrchestration.Services;

/// <summary>Calls PlantSafety.UI.Api's Workflow Orchestration console endpoint.</summary>
public sealed class WorkflowOrchestrationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Orders the given steps into a deterministic execution plan.</summary>
    public Task<string> BuildPlanAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/workflow-orchestration/build-plan", requestJson, cancellationToken);
}
