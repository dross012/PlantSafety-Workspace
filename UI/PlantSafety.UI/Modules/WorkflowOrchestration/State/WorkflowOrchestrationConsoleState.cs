// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.WorkflowOrchestration.State
//  File:           WorkflowOrchestrationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Workflow Orchestration console page.
// ============================================================================
using PlantSafety.UI.Modules.WorkflowOrchestration.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.WorkflowOrchestration.State;

/// <summary>State container for the Workflow Orchestration console page.</summary>
public sealed class WorkflowOrchestrationConsoleState(WorkflowOrchestrationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Builds the execution plan for the current step list.</summary>
    public Task BuildPlanAsync() => RunAsync(json => uiService.BuildPlanAsync(json));
}
