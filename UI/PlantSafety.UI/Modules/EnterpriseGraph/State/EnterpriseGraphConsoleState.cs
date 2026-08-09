// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.EnterpriseGraph.State
//  File:           EnterpriseGraphConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Enterprise Graph console page.
// ============================================================================
using PlantSafety.UI.Modules.EnterpriseGraph.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.EnterpriseGraph.State;

/// <summary>State container for the Enterprise Graph console page.</summary>
public sealed class EnterpriseGraphConsoleState(EnterpriseGraphUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Builds and publishes the current graph snapshot.</summary>
    public Task BuildAsync() => RunAsync(json => uiService.BuildAsync(json));
}
