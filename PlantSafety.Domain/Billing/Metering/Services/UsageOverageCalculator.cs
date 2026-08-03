// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           UsageOverageCalculator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1/§3.3 Usage Metering / Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Pure domain service computing, for a tenant's UsageMeteringSnapshot against its
//                  SubscriptionPlan's PlanLimits/OverageRates, the overage quantity and resulting
//                  overage charge for every metric. This is the highest-risk-of-being-wrong logic
//                  in the Billing module (per the task's own scope note), so it is deliberately a
//                  small set of pure, side-effect-free static functions with no I/O, fully unit
//                  testable in isolation from persistence/PDF/payment concerns. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Subscription.Aggregates;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.Metering.Services;

/// <summary>Result of overage-calculating a single metered dimension.</summary>
/// <param name="Metric">The usage dimension this result describes.</param>
/// <param name="RecordedUsage">The tenant's actual recorded usage for this metric in the period.</param>
/// <param name="PlanLimit">The plan's numeric limit for this metric, or <see langword="null"/> if unlimited.</param>
/// <param name="OverageQuantity">The portion of <see cref="RecordedUsage"/> exceeding <see cref="PlanLimit"/> (zero if within limit or unlimited).</param>
/// <param name="OverageCharge">The monetary charge for <see cref="OverageQuantity"/>, computed from the plan's <see cref="OverageRate"/> for this metric (zero if no overage or no configured rate).</param>
public sealed record MetricOverageResult(UsageMetricType Metric, decimal RecordedUsage, decimal? PlanLimit, decimal OverageQuantity, Money OverageCharge);

/// <summary>Pure domain service computing per-metric usage overage quantities and charges.</summary>
public static class UsageOverageCalculator
{
    /// <summary>
    /// Computes the overage result for every <see cref="UsageMetricType"/> present in
    /// <paramref name="snapshot"/>, against <paramref name="plan"/>'s limits and overage rates.
    /// </summary>
    public static IReadOnlyList<MetricOverageResult> ComputeAll(UsageMeteringSnapshot snapshot, SubscriptionPlan plan, string currency)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        return UsageMetricType.All.Select(metric => ComputeOne(snapshot, plan, metric, currency)).ToList();
    }

    /// <summary>Computes the overage result for a single metric.</summary>
    public static MetricOverageResult ComputeOne(UsageMeteringSnapshot snapshot, SubscriptionPlan plan, UsageMetricType metric, string currency)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(metric);

        var recorded = snapshot.GetValue(metric);
        decimal? limit = ResolvePlanLimit(plan, metric);

        var overageQuantity = limit.HasValue ? Math.Max(0m, recorded - limit.Value) : 0m;

        var rate = plan.FindOverageRate(metric);
        var charge = rate is null ? Money.Zero(currency) : rate.ComputeCharge(overageQuantity);

        return new MetricOverageResult(metric, recorded, limit, overageQuantity, charge);
    }

    /// <summary>Resolves the numeric plan limit for the given metric, or <see langword="null"/> if that metric is unlimited/not capped under this plan.</summary>
    private static decimal? ResolvePlanLimit(SubscriptionPlan plan, UsageMetricType metric)
    {
        if (metric.Equals(UsageMetricType.UserCount))
            return plan.Limits.MaxUsers;
        if (metric.Equals(UsageMetricType.StorageBytes))
            return plan.Limits.StorageQuotaBytes;
        if (metric.Equals(UsageMetricType.ScheduledTasks))
            return plan.Limits.MaxScheduledTasksPerMonth;
        if (metric.Equals(UsageMetricType.EventVolume))
            return plan.Limits.MaxEventVolumePerMonth;

        // ApiCalls and ModuleUsage have no fixed numeric PlanLimits field in this catalog --
        // ApiCalls is throttled in real time via per-minute rate limiting (ApiRateLimitPerMinute),
        // not a monthly overage cap; ModuleUsage is enforced via AllowedModuleCodes, a set
        // membership check, not a numeric cap -- unlimited for overage-billing purposes.
        return null;
    }
}
