// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.ComplianceRisk.Services
//  File:           ComplianceRiskUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's ComplianceRiskController --
//                   Shared.SafetyData's real Compliance & Risk subsystem,
//                   including Phase C.10's severity-aware scoring (wired up
//                   after being flagged as a known gap in the original
//                   PlantSafety.UI build-out).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.ComplianceRisk.Services;

/// <summary>Calls PlantSafety.UI.Api's Compliance &amp; Risk assessment endpoint.</summary>
public sealed class ComplianceRiskUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Assesses a facility's severity-aware compliance and risk scores from the given input JSON.</summary>
    public Task<string> AssessAsync(string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync("api/v1/ui/compliance-risk/assess", requestJson, cancellationToken);
}
