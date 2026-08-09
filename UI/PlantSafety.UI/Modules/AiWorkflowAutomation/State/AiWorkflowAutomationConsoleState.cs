// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiWorkflowAutomation.State
//  File:           AiWorkflowAutomationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Workflow Automation console page.
// ============================================================================
using PlantSafety.UI.Modules.AiWorkflowAutomation.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiWorkflowAutomation.State;

/// <summary>State container for the AI Workflow Automation console page.</summary>
public sealed class AiWorkflowAutomationConsoleState(AiWorkflowAutomationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Automates workflow artifacts for the current input JSON.</summary>
    public Task AutomateAsync() => RunAsync(json => uiService.AutomateAsync(json));
}
