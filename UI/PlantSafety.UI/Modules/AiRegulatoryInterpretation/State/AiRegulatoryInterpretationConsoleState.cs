// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiRegulatoryInterpretation.State
//  File:           AiRegulatoryInterpretationConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Regulatory Interpretation console page.
// ============================================================================
using PlantSafety.UI.Modules.AiRegulatoryInterpretation.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiRegulatoryInterpretation.State;

/// <summary>State container for the AI Regulatory Interpretation console page.</summary>
public sealed class AiRegulatoryInterpretationConsoleState(AiRegulatoryInterpretationUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Interprets applicable jurisdictions for the current input JSON.</summary>
    public Task InterpretAsync() => RunAsync(json => uiService.InterpretAsync(json));
}
