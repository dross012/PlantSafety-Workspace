// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiPredictiveOptimization.State
//  File:           AiPredictiveOptimizationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Predictive Optimization console page.
// ============================================================================
using PlantSafety.UI.Modules.AiPredictiveOptimization.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiPredictiveOptimization.State;

/// <summary>State container for the AI Predictive Optimization console page.</summary>
public sealed class AiPredictiveOptimizationConsoleState(AiPredictiveOptimizationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Optimizes every target dimension in the current input JSON.</summary>
    public Task OptimizeAsync() => RunAsync(json => uiService.OptimizeAsync(json));
}
