// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiHazardUnderstanding.Services
//  File:           AiHazardUnderstandingUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's HazardUnderstandingController (AI.HazardUnderstanding, Phase E.2).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiHazardUnderstanding.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Hazard Understanding console endpoint.</summary>
public sealed class AiHazardUnderstandingUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Builds a semantic hazard-understanding model from the given input JSON.</summary>
    public Task<string> UnderstandAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/hazard-understanding/understand", requestJson, cancellationToken);
}
