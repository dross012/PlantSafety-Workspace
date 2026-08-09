// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.WorkflowEngine.State
//  File:           WorkflowEngineConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Workflow Engine console page.
// ============================================================================
using PlantSafety.UI.Modules.WorkflowEngine.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.WorkflowEngine.State;

/// <summary>State container for the Workflow Engine console page.</summary>
public sealed class WorkflowEngineConsoleState(WorkflowEngineUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Evaluates the current request JSON.</summary>
    public Task EvaluateAsync() => RunAsync(json => uiService.EvaluateAsync(json));
}
