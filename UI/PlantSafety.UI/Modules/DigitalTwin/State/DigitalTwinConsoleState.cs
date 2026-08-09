// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.DigitalTwin.State
//  File:           DigitalTwinConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Digital Twin console page.
// ============================================================================
using PlantSafety.UI.Modules.DigitalTwin.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.DigitalTwin.State;

/// <summary>State container for the Digital Twin console page.</summary>
public sealed class DigitalTwinConsoleState(DigitalTwinUiService uiService) : EngineConsoleStateBase
{
    /// <summary>The currently selected simulation endpoint.</summary>
    public string SelectedEndpoint { get; set; } = "severity-progression";

    /// <summary>Runs the currently selected simulation with the current request JSON.</summary>
    public Task RunSelectedAsync() => RunAsync(json => uiService.RunAsync(SelectedEndpoint, json));
}
