// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.PredictiveModels.Services
//  File:           PredictiveModelsUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's PredictiveModelsController. One
//                  method per model; the console page's endpoint selector
//                  picks which one to call.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.PredictiveModels.Services;

/// <summary>Calls PlantSafety.UI.Api's Predictive Models console endpoints.</summary>
public sealed class PredictiveModelsUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Runs the given model endpoint (hazard-likelihood/severity-likelihood/exposure-likelihood/machine-failure/employee-injury) with the given request JSON.</summary>
    public Task<string> RunAsync(string endpoint, string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync($"api/v1/ui/engines/predictive-models/{endpoint}", requestJson, cancellationToken);
}
