// AUTHOR: PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Maps SubscriptionPlan domain aggregates to their canonical read model.
// SchemaVersion: 1

using PlantSafety.Application.Billing.Subscription.Models;
using PlantSafety.Domain.Billing.Subscription.Aggregates;

namespace PlantSafety.Application.Billing.Subscription.Mapping;

/// <summary>Maps <see cref="SubscriptionPlan"/> aggregates to <see cref="SubscriptionPlanDto"/>.</summary>
public static class SubscriptionPlanMapper
{
    /// <summary>Maps a SubscriptionPlan to its canonical read model.</summary>
    public static SubscriptionPlanDto ToDto(SubscriptionPlan plan) => new(
        plan.Tier.Code,
        plan.DisplayName,
        plan.MonthlyBasePrice.Amount,
        plan.MonthlyBasePrice.Currency,
        plan.Limits.MaxUsers,
        plan.Limits.ApiRateLimitPerMinute,
        plan.Limits.StorageQuotaBytes,
        plan.Limits.AllowedModuleCodes,
        plan.Limits.MaxScheduledTasksPerMonth,
        plan.Limits.MaxEventVolumePerMonth,
        plan.IsActive);
}
