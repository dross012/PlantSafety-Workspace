// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.ComplianceRisk.State
//  File:           ComplianceRiskAssessState.cs
//  Created:        2026-08-09
//  Description:    State container for the Compliance & Risk assessment console page.
// ============================================================================
using PlantSafety.UI.Modules.ComplianceRisk.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.ComplianceRisk.State;

/// <summary>State container for the Compliance &amp; Risk assessment console page.</summary>
public sealed class ComplianceRiskAssessState(ComplianceRiskUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Assesses the current input JSON.</summary>
    public Task AssessAsync() => RunAsync(json => uiService.AssessAsync(json));
}
