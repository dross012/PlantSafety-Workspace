// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiEnterpriseOrchestration.Services
//  File:           AiEnterpriseOrchestrationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's EnterpriseOrchestrationController (AI.EnterpriseOrchestration, Phase E.9).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.AiEnterpriseOrchestration.Services;

/// <summary>Calls PlantSafety.UI.Api's AI Enterprise Orchestration console endpoint.</summary>
public sealed class AiEnterpriseOrchestrationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Orchestrates across every facility in the given input JSON.</summary>
    public Task<string> OrchestrateAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/engines/enterprise-orchestration/orchestrate", requestJson, cancellationToken);
}
