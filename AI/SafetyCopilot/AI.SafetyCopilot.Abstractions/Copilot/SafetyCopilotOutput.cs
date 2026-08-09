// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Copilot
//  File:           SafetyCopilotOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.8: the result of a safety-copilot session --
//                  every actionable item and explanatory recommendation
//                  generated for the requested audience.
// ============================================================================
using AI.SafetyCopilot.Abstractions.Recommendations;

namespace AI.SafetyCopilot.Abstractions.Copilot;

/// <summary>The result of a safety-copilot session.</summary>
public sealed record SafetyCopilotOutput(
    IReadOnlyList<SafetyCopilotAction> Actions,
    IReadOnlyList<SafetyCopilotRecommendation> Recommendations,
    DateTime GeneratedAtUtc);
