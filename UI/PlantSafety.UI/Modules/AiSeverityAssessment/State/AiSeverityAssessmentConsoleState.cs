// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiSeverityAssessment.State
//  File:           AiSeverityAssessmentConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Severity Assessment console page.
// ============================================================================
using PlantSafety.UI.Modules.AiSeverityAssessment.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiSeverityAssessment.State;

/// <summary>State container for the AI Severity Assessment console page.</summary>
public sealed class AiSeverityAssessmentConsoleState(AiSeverityAssessmentUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Assesses severity for the current input JSON.</summary>
    public Task AssessAsync() => RunAsync(json => uiService.AssessAsync(json));
}
