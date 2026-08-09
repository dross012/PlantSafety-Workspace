// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiRegulatoryInterpretation.Services
//  File:           AiRegulatoryInterpretationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's RegulatoryInterpretationController (AI.RegulatoryInterpretation, Phase E.4).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiRegulatoryInterpretation.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Regulatory Interpretation console endpoint.</summary>
public sealed class AiRegulatoryInterpretationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Interprets applicable jurisdictions from the given input JSON.</summary>
    public Task<string> InterpretAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/regulatory-interpretation/interpret", requestJson, cancellationToken);
}
