// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.PredictiveModeling.State
//  File:           PredictiveModelingConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Predictive Modeling console page.
// ============================================================================
using PlantSafety.UI.Modules.PredictiveModeling.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.PredictiveModeling.State;

/// <summary>State container for the Predictive Modeling console page.</summary>
public sealed class PredictiveModelingConsoleState(PredictiveModelingUiService uiService) : EngineConsoleStateBase
{
    /// <summary>The currently selected forecast endpoint.</summary>
    public string SelectedEndpoint { get; set; } = "risk";

    /// <summary>Runs the currently selected forecast with the current request JSON.</summary>
    public Task RunSelectedAsync() => RunAsync(json => uiService.RunAsync(SelectedEndpoint, json));
}
