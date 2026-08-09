// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.DigitalTwin.Services
//  File:           DigitalTwinUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's DigitalTwinController. One
//                  method per simulation; the console page's simulation
//                  selector picks which one to call.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.DigitalTwin.Services;

/// <summary>Calls PlantSafety.UI.Api's Digital Twin console endpoints.</summary>
public sealed class DigitalTwinUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Runs the given simulation endpoint (hazard-propagation/severity-progression/workflow/multi-hazard-chain/facility-lifecycle) with the given request JSON.</summary>
    public Task<string> RunAsync(string endpoint, string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync($"api/v1/ui/engines/digital-twin/{endpoint}", requestJson, cancellationToken);
}
