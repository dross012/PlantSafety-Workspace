// AUTHOR: PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical read model for a SubscriptionPlan catalog entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Billing.Subscription.Models;

/// <summary>Canonical read model for a <see cref="Domain.Billing.Subscription.Aggregates.SubscriptionPlan"/> catalog entry.</summary>
/// <param name="PlanTierCode">Stable machine-readable plan tier code (Trial/Standard/Professional/Enterprise/Custom).</param>
/// <param name="DisplayName">Human-readable plan name.</param>
/// <param name="MonthlyBasePriceAmount">Base recurring monthly price amount.</param>
/// <param name="Currency">ISO-4217 currency code for all monetary fields.</param>
/// <param name="MaxUsers">Maximum active user accounts permitted, or <see langword="null"/> if unlimited.</param>
/// <param name="ApiRateLimitPerMinute">Maximum API requests per minute before throttling.</param>
/// <param name="StorageQuotaBytes">Maximum blob storage in bytes, or <see langword="null"/> if unlimited.</param>
/// <param name="AllowedModuleCodes">The closed set of module identifiers this plan permits, or <see langword="null"/> if every module is permitted.</param>
/// <param name="MaxScheduledTasksPerMonth">Maximum scheduled task runs per billing period, or <see langword="null"/> if unlimited.</param>
/// <param name="MaxEventVolumePerMonth">Maximum integration event volume per billing period, or <see langword="null"/> if unlimited.</param>
/// <param name="IsActive">Whether this plan is currently offered for self-service selection.</param>
public sealed record SubscriptionPlanDto(
    string PlanTierCode,
    string DisplayName,
    decimal MonthlyBasePriceAmount,
    string Currency,
    int? MaxUsers,
    int ApiRateLimitPerMinute,
    long? StorageQuotaBytes,
    IReadOnlyList<string>? AllowedModuleCodes,
    int? MaxScheduledTasksPerMonth,
    long? MaxEventVolumePerMonth,
    bool IsActive);
