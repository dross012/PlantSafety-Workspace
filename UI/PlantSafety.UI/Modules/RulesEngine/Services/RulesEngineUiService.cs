// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.RulesEngine.Services
//  File:           RulesEngineUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's RulesEngineController.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.RulesEngine.Services;

/// <summary>Calls PlantSafety.UI.Api's Rules Engine console endpoint.</summary>
public sealed class RulesEngineUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Evaluates the given rules/facts JSON.</summary>
    public Task<string> EvaluateAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/rules/evaluate", requestJson, cancellationToken);
}
