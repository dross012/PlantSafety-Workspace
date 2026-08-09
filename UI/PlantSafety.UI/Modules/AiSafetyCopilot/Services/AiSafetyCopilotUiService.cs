// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiSafetyCopilot.Services
//  File:           AiSafetyCopilotUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's SafetyCopilotController (AI.SafetyCopilot, Phase E.8).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiSafetyCopilot.Services;

/// <summary>The typed request the Copilot dashboard submits.</summary>
public sealed record SafetyCopilotGuidanceRequest(
    string Audience,
    string HazardType,
    string OriginPropagationStage,
    string SeverityLevel,
    string AffectedScope,
    int EmployeeInjuryCount,
    int EmployeeExposureCount,
    int MachineFailureCount,
    int EventChainLength,
    string DigitalTwinSimulatedSeverity);

/// <summary>One guidance action returned by the copilot.</summary>
public sealed record SafetyCopilotActionDto(string ActionCode, string Description, string Audience);

/// <summary>One guidance recommendation returned by the copilot.</summary>
public sealed record SafetyCopilotRecommendationDto(string Title, string Guidance, string Audience);

/// <summary>The copilot's full guidance output.</summary>
public sealed record SafetyCopilotOutputDto(IReadOnlyList<SafetyCopilotActionDto> Actions, IReadOnlyList<SafetyCopilotRecommendationDto> Recommendations, DateTime GeneratedAtUtc);

/// <summary>Calls PlantSafety.UI.Api's AI Safety Copilot endpoint.</summary>
public sealed class AiSafetyCopilotUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Requests tailored guidance for the given audience/context.</summary>
    public Task<SafetyCopilotOutputDto?> GuideAsync(SafetyCopilotGuidanceRequest request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<SafetyCopilotGuidanceRequest, SafetyCopilotOutputDto>("api/v1/ui/engines/safety-copilot/guide", request, cancellationToken);
}
