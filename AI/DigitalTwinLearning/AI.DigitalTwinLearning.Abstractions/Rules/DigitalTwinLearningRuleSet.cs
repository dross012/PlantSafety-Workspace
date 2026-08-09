// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Rules
//  File:           DigitalTwinLearningRuleSet.cs
//  Created:        2026-08-09
//  Description:    The standard, deterministic rule set for
//                  DigitalTwinLearningEngine.Learn.
// ============================================================================
using AI.DigitalTwinLearning.Abstractions.Learning;

namespace AI.DigitalTwinLearning.Abstractions.Rules;

/// <summary>The standard rule set applied to every learned pattern.</summary>
public static class DigitalTwinLearningRuleSet
{
    /// <summary>The standard, deterministic rules.</summary>
    public static IReadOnlyList<DigitalTwinLearningRule> Standard { get; } =
    [
        new DigitalTwinLearningRule(
            "DTL-001",
            "Anomalous pattern",
            pattern => pattern.IsAnomalous,
            "Anomalous occurrence detected: recalibrate the digital twin's baseline for this pattern before trusting further simulations."),

        new DigitalTwinLearningRule(
            "DTL-002",
            "Rising trend",
            pattern => pattern.TrendSlope > 0.5,
            "Occurrences are trending upward significantly: increase the digital twin's simulated frequency for this pattern."),

        new DigitalTwinLearningRule(
            "DTL-003",
            "Falling trend",
            pattern => pattern.TrendSlope < -0.5,
            "Occurrences are trending downward significantly: decrease the digital twin's simulated frequency for this pattern."),

        new DigitalTwinLearningRule(
            "DTL-004",
            "Sustained elevated baseline",
            pattern => pattern.MovingAverage >= 3.0,
            "Moving average has settled at an elevated level: treat this as the new normal baseline, not a transient spike."),
    ];
}
