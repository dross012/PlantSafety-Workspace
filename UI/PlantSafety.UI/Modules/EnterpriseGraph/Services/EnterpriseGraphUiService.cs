// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.EnterpriseGraph.Services
//  File:           EnterpriseGraphUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's GraphController (build/publish)
//                  and Enterprise.ApiGateway's reused /graph GET (read).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.EnterpriseGraph.Services;

/// <summary>Calls PlantSafety.UI.Api's Enterprise Graph endpoints.</summary>
public sealed class EnterpriseGraphUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>(Re)builds and publishes the current enterprise graph snapshot from the given nodes/edges JSON.</summary>
    public Task<string> BuildAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/graph", requestJson, cancellationToken);
}
