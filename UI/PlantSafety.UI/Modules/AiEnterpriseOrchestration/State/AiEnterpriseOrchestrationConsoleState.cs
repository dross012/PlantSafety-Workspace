// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiEnterpriseOrchestration.State
//  File:           AiEnterpriseOrchestrationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Enterprise Orchestration console page.
// ============================================================================
using PlantSafety.UI.Modules.AiEnterpriseOrchestration.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiEnterpriseOrchestration.State;

/// <summary>State container for the AI Enterprise Orchestration console page.</summary>
public sealed class AiEnterpriseOrchestrationConsoleState(AiEnterpriseOrchestrationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Orchestrates across every facility for the current input JSON.</summary>
    public Task OrchestrateAsync() => RunAsync(json => uiService.OrchestrateAsync(json));
}
