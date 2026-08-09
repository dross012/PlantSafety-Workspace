// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.EnterpriseDashboard.Services
//  File:           EnterpriseDashboardUiService.cs
//  Created:        2026-08-09
//  Description:    Calls Enterprise.Dashboard's reused EnterpriseDashboardController
//                  (mounted as an ApplicationPart in PlantSafety.UI.Api).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.EnterpriseDashboard.Services;

/// <summary>Calls PlantSafety.UI.Api's reused Enterprise Dashboard endpoint.</summary>
public sealed class EnterpriseDashboardUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Fetches the aggregated enterprise dashboard payload as formatted JSON.</summary>
    public async Task<string> GetDashboardJsonAsync(CancellationToken cancellationToken = default)
    {
        object? dto = await apiClient.GetAsync<object>("api/v1/enterprise/dashboard", cancellationToken);
        return System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
