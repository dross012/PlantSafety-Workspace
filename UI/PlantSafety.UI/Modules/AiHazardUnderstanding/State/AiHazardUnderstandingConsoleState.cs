// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiHazardUnderstanding.State
//  File:           AiHazardUnderstandingConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Hazard Understanding console page.
// ============================================================================
using PlantSafety.UI.Modules.AiHazardUnderstanding.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiHazardUnderstanding.State;

/// <summary>State container for the AI Hazard Understanding console page.</summary>
public sealed class AiHazardUnderstandingConsoleState(AiHazardUnderstandingUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Builds the semantic hazard-understanding model for the current input JSON.</summary>
    public Task UnderstandAsync() => RunAsync(json => uiService.UnderstandAsync(json));
}
