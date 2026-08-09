// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Copilot
//  File:           CopilotAudience.cs
//  Created:        2026-08-08
//  Description:    Phase E.8 (Path E: AI-Driven Safety Automation): who a
//                  SafetyCopilotAction/SafetyCopilotRecommendation is
//                  addressed to -- Path E's spec explicitly requires
//                  guiding both employees and supervisors, with different
//                  content for each.
// ============================================================================
namespace AI.SafetyCopilot.Abstractions.Copilot;

/// <summary>Who a copilot action/recommendation is addressed to.</summary>
public enum CopilotAudience
{
    /// <summary>A frontline employee.</summary>
    Employee,

    /// <summary>A supervisor.</summary>
    Supervisor,
}
