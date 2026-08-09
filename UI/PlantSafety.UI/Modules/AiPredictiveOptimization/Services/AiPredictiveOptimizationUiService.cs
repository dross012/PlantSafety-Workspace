// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiPredictiveOptimization.Services
//  File:           AiPredictiveOptimizationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's PredictiveOptimizationController (AI.PredictiveOptimization, Phase E.6).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiPredictiveOptimization.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Predictive Optimization console endpoint.</summary>
public sealed class AiPredictiveOptimizationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Optimizes every target dimension in the given input JSON.</summary>
    public Task<string> OptimizeAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/predictive-optimization/optimize", requestJson, cancellationToken);
}
