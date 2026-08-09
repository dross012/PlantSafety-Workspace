// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiDigitalTwinLearning.State
//  File:           AiDigitalTwinLearningConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Digital Twin Learning console page.
// ============================================================================
using PlantSafety.UI.Modules.AiDigitalTwinLearning.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiDigitalTwinLearning.State;

/// <summary>State container for the AI Digital Twin Learning console page.</summary>
public sealed class AiDigitalTwinLearningConsoleState(AiDigitalTwinLearningUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Learns every pattern category for the current input JSON.</summary>
    public Task LearnAsync() => RunAsync(json => uiService.LearnAsync(json));
}
