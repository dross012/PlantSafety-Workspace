// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           SustainabilityTargetRiskPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy evaluating whether a SustainabilityTarget's tracked progress
//                  should be classified as at-risk. Risk hook: an at-risk classification is the
//                  trigger AdvancedMetricsLedger.SetSustainabilityTarget/UpdateTargetProgress uses
//                  to raise SustainabilityTargetAtRiskEvent for RiskManagement consumption.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Policies;

/// <summary>Evaluates whether a <see cref="Entities.SustainabilityTarget"/>'s progress is at risk of missing its target date.</summary>
public static class SustainabilityTargetRiskPolicy
{
    /// <summary>
    /// Fraction of the baseline-to-target gap that must have closed by "now" (linear pacing,
    /// pro-rated between SetAtUtc and TargetDateUtc) before a target is considered on track.
    /// Progress below this pace, with time remaining, is classified as at-risk.
    /// </summary>
    private const decimal MinimumOnTrackPaceRatio = 0.85m;

    /// <summary>
    /// Evaluates the target's current progress against its expected linear pace and returns the
    /// resulting status. Assumes the tracked KPI decreases toward the target (e.g. intensity,
    /// emissions); increasing-is-better KPIs (e.g. diversion rate) invert the direction check.
    /// </summary>
    public static SustainabilityTargetStatus Evaluate(
        Entities.SustainabilityTarget target,
        decimal currentValue,
        UtcTimestamp nowUtc,
        bool higherIsBetter = false)
    {
        if (nowUtc.Value > target.TargetDateUtc.Value)
            return IsGoalMet(target.BaselineValue, target.TargetValue, currentValue, higherIsBetter)
                ? SustainabilityTargetStatus.Achieved
                : SustainabilityTargetStatus.Missed;

        if (IsGoalMet(target.BaselineValue, target.TargetValue, currentValue, higherIsBetter))
            return SustainabilityTargetStatus.Achieved;

        var totalSpanTicks = (target.TargetDateUtc.Value - target.SetAtUtc.Value).Ticks;
        if (totalSpanTicks <= 0)
            return SustainabilityTargetStatus.OnTrack;

        var elapsedRatio = Math.Clamp(
            (decimal)(nowUtc.Value - target.SetAtUtc.Value).Ticks / totalSpanTicks, 0m, 1m);

        var totalDelta = target.TargetValue - target.BaselineValue;
        if (totalDelta == 0)
            return SustainabilityTargetStatus.OnTrack;

        var expectedValue = target.BaselineValue + (totalDelta * elapsedRatio);
        var actualProgressRatio = SafeRatio(currentValue - target.BaselineValue, expectedValue - target.BaselineValue);

        return actualProgressRatio >= MinimumOnTrackPaceRatio
            ? SustainabilityTargetStatus.OnTrack
            : SustainabilityTargetStatus.AtRisk;
    }

    private static bool IsGoalMet(decimal baseline, decimal target, decimal current, bool higherIsBetter)
        => higherIsBetter ? current >= target : current <= target;

    private static decimal SafeRatio(decimal numerator, decimal denominator)
        => denominator == 0 ? 1m : numerator / denominator;
}
