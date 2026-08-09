// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Copilot
//  File:           SafetyCopilotAction.cs
//  Created:        2026-08-08
//  Description:    Phase E.8: a single actionable item the copilot
//                  surfaces to its audience.
// ============================================================================
namespace AI.SafetyCopilot.Abstractions.Copilot;

/// <summary>A single actionable item the copilot surfaces to its audience.</summary>
public sealed record SafetyCopilotAction(
    string ActionCode,
    string Description,
    CopilotAudience Audience);
