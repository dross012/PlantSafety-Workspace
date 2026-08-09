// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.AiReasoning.State
//  File:           AiReasoningConsoleState.cs
//  Created:        2026-08-09
//  Description:    State container for the AI Reasoning console page.
// ============================================================================
using PlantSafety.UI.Modules.AiReasoning.Services;
using PlantSafety.UI.State;

namespace PlantSafety.UI.Modules.AiReasoning.State;

/// <summary>State container for the AI Reasoning console page.</summary>
public sealed class AiReasoningConsoleState(AiReasoningUiService uiService) : EngineConsoleStateBase
{
    /// <summary>Reasons over the current signal-set JSON.</summary>
    public Task ReasonAsync() => RunAsync(json => uiService.ReasonAsync(json));
}
