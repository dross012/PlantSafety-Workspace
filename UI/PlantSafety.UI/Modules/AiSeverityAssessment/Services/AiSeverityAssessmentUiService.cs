// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiSeverityAssessment.Services
//  File:           AiSeverityAssessmentUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's SeverityAssessmentController (AI.SeverityAssessment, Phase E.5).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiSeverityAssessment.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Severity Assessment console endpoint.</summary>
public sealed class AiSeverityAssessmentUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Assesses severity from the given input JSON.</summary>
    public Task<string> AssessAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/severity-assessment/assess", requestJson, cancellationToken);
}
