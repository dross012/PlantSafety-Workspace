// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiDigitalTwinLearning.Services
//  File:           AiDigitalTwinLearningUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's DigitalTwinLearningController (AI.DigitalTwinLearning, Phase E.7).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiDigitalTwinLearning.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Digital Twin Learning console endpoint.</summary>
public sealed class AiDigitalTwinLearningUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Learns every pattern category in the given input JSON.</summary>
    public Task<string> LearnAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/digital-twin-learning/learn", requestJson, cancellationToken);
}
