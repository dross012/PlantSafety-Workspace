// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.PredictiveModeling.Services
//  File:           PredictiveModelingUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's PredictiveModelingController.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.PredictiveModeling.Services;

/// <summary>Calls PlantSafety.UI.Api's Predictive Modeling (enterprise-scope) console endpoints.</summary>
public sealed class PredictiveModelingUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Runs the given forecast endpoint (risk/compliance/hazard/severity) with the given request JSON.</summary>
    public Task<string> RunAsync(string endpoint, string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync($"api/v1/ui/engines/predictive-modeling/{endpoint}", requestJson, cancellationToken);
}
