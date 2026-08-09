// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.PredictiveModels.State
//  File:           PredictiveModelsConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Predictive Models console page.
// ============================================================================
using PlantSafety.UI.Modules.PredictiveModels.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.PredictiveModels.State;

/// <summary>State container for the Predictive Models console page.</summary>
public sealed class PredictiveModelsConsoleState(PredictiveModelsUiService uiService) : EngineConsoleStateBase
{
    /// <summary>The currently selected model endpoint.</summary>
    public string SelectedEndpoint { get; set; } = "hazard-likelihood";

    /// <summary>Runs the currently selected model with the current request JSON.</summary>
    public Task RunSelectedAsync() => RunAsync(json => uiService.RunAsync(SelectedEndpoint, json));
}
