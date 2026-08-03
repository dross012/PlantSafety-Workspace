// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           PlanLimits.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans / §3.4 License Enforcement)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object holding a SubscriptionPlan's concrete, enforceable
//                  limits: user count cap, API rate limit, allowed module list, and storage
//                  quota. Consumed directly by license-enforcement call sites (CreateUserCommandHandler,
//                  SetTenantFeatureFlagOverrideCommandHandler, the per-tenant ASP.NET Core rate
//                  limiter policy) -- reference data, not logic scattered across those call sites.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.Subscription.ValueObjects;

/// <summary>Concrete, enforceable limits carried by a <see cref="Aggregates.SubscriptionPlan"/>.</summary>
/// <param name="MaxUsers">
/// Maximum number of active user accounts the tenant may provision under this plan.
/// <see langword="null"/> means unlimited (Enterprise/Custom tiers only).
/// </param>
/// <param name="ApiRateLimitPerMinute">Maximum API requests per minute the tenant's traffic may sustain before HTTP 429 throttling applies.</param>
/// <param name="StorageQuotaBytes">Maximum blob storage, in bytes, the tenant may consume. <see langword="null"/> means unlimited.</param>
/// <param name="AllowedModuleCodes">
/// The closed set of feature-flag-gated module identifiers this plan permits the tenant to enable
/// (matches <c>TenantConfiguration.EnabledModules</c> identifiers). <see langword="null"/> means
/// every module is permitted (Enterprise/Custom tiers only).
/// </param>
/// <param name="MaxScheduledTasksPerMonth">Maximum scheduled/optimization task runs the tenant may execute in a billing period. <see langword="null"/> means unlimited.</param>
/// <param name="MaxEventVolumePerMonth">Maximum integration/domain events the tenant may generate in a billing period. <see langword="null"/> means unlimited.</param>
public sealed record PlanLimits(
    int? MaxUsers,
    int ApiRateLimitPerMinute,
    long? StorageQuotaBytes,
    IReadOnlyList<string>? AllowedModuleCodes,
    int? MaxScheduledTasksPerMonth = null,
    long? MaxEventVolumePerMonth = null)
{
    /// <summary>Returns <see langword="true"/> when <paramref name="currentUserCount"/> has already reached or exceeded <see cref="MaxUsers"/>.</summary>
    public bool IsUserCapReached(int currentUserCount) => MaxUsers.HasValue && currentUserCount >= MaxUsers.Value;

    /// <summary>Returns <see langword="true"/> when <paramref name="moduleCode"/> is permitted under this plan.</summary>
    public bool IsModuleAllowed(string moduleCode)
        => AllowedModuleCodes is null || AllowedModuleCodes.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns <see langword="true"/> when <paramref name="currentStorageBytes"/> has already reached or exceeded <see cref="StorageQuotaBytes"/>.</summary>
    public bool IsStorageQuotaExceeded(long currentStorageBytes) => StorageQuotaBytes.HasValue && currentStorageBytes >= StorageQuotaBytes.Value;
}
