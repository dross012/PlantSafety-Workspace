// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.RulesEngine.State
//  File:           RulesEngineConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the Rules Engine console page.
// ============================================================================
using PlantSafety.UI.Modules.RulesEngine.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.RulesEngine.State;

/// <summary>State container for the Rules Engine console page.</summary>
public sealed class RulesEngineConsoleState(RulesEngineUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Evaluates the current rules/facts JSON.</summary>
    public Task EvaluateAsync() => RunAsync(json => uiService.EvaluateAsync(json));
}
