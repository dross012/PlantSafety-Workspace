// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Recommendations
//  File:           SafetyCopilotRecommendation.cs
//  Created:        2026-08-08
//  Description:    Phase E.8: a single explanatory recommendation the
//                  copilot surfaces to its audience -- covers hazard
//                  explanations, regulatory guidance, and severity
//                  assessment summaries per Path E's spec.
// ============================================================================
using AI.SafetyCopilot.Abstractions.Copilot;

namespace AI.SafetyCopilot.Abstractions.Recommendations;

/// <summary>A single explanatory recommendation the copilot surfaces to its audience.</summary>
public sealed record SafetyCopilotRecommendation(
    string Title,
    string Guidance,
    CopilotAudience Audience);
