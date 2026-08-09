// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Integration.State
//  File:           IntegrationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Integration console page.
// ============================================================================
using PlantSafety.UI.Modules.Integration.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.Integration.State;

/// <summary>State container for the Integration console page.</summary>
public sealed class IntegrationConsoleState(IntegrationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>The currently selected source system.</summary>
    public string SelectedSystem { get; set; } = "Hr";

    /// <summary>Ingests an event from the currently selected system.</summary>
    public Task IngestAsync() => RunAsync(json => uiService.IngestAsync(SelectedSystem, json));
}
