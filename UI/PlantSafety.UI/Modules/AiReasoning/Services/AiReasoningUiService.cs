// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiReasoning.Services
//  File:           AiReasoningUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's ReasoningController (AI.Reasoning, Phase E.1).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiReasoning.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Reasoning console endpoint.</summary>
public sealed class AiReasoningUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Reasons over the given signal-set JSON.</summary>
    public Task<string> ReasonAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/reasoning/reason", requestJson, cancellationToken);
}
