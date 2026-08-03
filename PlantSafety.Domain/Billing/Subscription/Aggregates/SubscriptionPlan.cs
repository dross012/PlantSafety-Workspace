// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           SubscriptionPlan.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
//  SchemaVersion:  2026.1
//  Description:    Reference-data aggregate describing a subscription plan tier's commercial terms:
//                  base monthly price, enforceable limits (PlanLimits), and per-metric overage
//                  pricing (OverageRate). Instances are NOT created ad-hoc by callers -- the closed
//                  catalog of currently-offered plans lives in SubscriptionPlanCatalog (mirroring
//                  the existing SubscriptionPlanCode closed-set convention on Tenant), and this
//                  class exposes only a private constructor plus the internal factory the catalog
//                  uses, so no code outside the catalog can fabricate an ad-hoc "plan" with
//                  arbitrary limits. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.Subscription.Enums;
using PlantSafety.Domain.Billing.Subscription.ValueObjects;
using PlantSafety.Domain.Billing.Subscription.ValueObjects.Ids;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.Subscription.Aggregates;

/// <summary>Reference-data aggregate describing one subscription plan's commercial terms and enforceable limits.</summary>
public sealed class SubscriptionPlan
{
    /// <summary>Strong-typed, deterministic identity of this plan (see <see cref="SubscriptionPlanId.FromCode"/>).</summary>
    public SubscriptionPlanId SubscriptionPlanId { get; }

    /// <summary>Commercial tier this plan represents.</summary>
    public PlanTier Tier { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Base recurring monthly price for this plan, before any usage-based overage or discounts/credits.</summary>
    public Money MonthlyBasePrice { get; }

    /// <summary>Concrete, enforceable limits for this plan.</summary>
    public PlanLimits Limits { get; }

    /// <summary>Per-metric overage pricing applied once a tenant's usage exceeds <see cref="Limits"/> for that metric.</summary>
    public IReadOnlyList<OverageRate> OverageRates { get; }

    /// <summary>Whether this plan is currently offered to new tenants (Custom plans are always inactive for self-service selection).</summary>
    public bool IsActive { get; }

    private SubscriptionPlan(
        SubscriptionPlanId subscriptionPlanId, PlanTier tier, string displayName, Money monthlyBasePrice,
        PlanLimits limits, IReadOnlyList<OverageRate> overageRates, bool isActive)
    {
        SubscriptionPlanId = subscriptionPlanId;
        Tier = tier;
        DisplayName = displayName;
        MonthlyBasePrice = monthlyBasePrice;
        Limits = limits;
        OverageRates = overageRates;
        IsActive = isActive;
    }

    /// <summary>
    /// Constructs a SubscriptionPlan reference-data entry. Internal -- only
    /// <see cref="Catalog.SubscriptionPlanCatalog"/> may construct instances, keeping the set of
    /// plans a closed, auditable catalog rather than an ad-hoc value any caller can fabricate.
    /// </summary>
    internal static SubscriptionPlan Create(
        PlanTier tier, string displayName, Money monthlyBasePrice, PlanLimits limits,
        IReadOnlyList<OverageRate> overageRates, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(tier);
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("SubscriptionPlan.DisplayName must not be empty.", nameof(displayName));
        ArgumentNullException.ThrowIfNull(limits);
        ArgumentNullException.ThrowIfNull(overageRates);

        return new SubscriptionPlan(SubscriptionPlanId.FromCode(tier.Code), tier, displayName, monthlyBasePrice, limits, overageRates, isActive);
    }

    /// <summary>Returns the overage rate configured for the given metric, or <see langword="null"/> if that metric is not overage-priced under this plan.</summary>
    public OverageRate? FindOverageRate(Metering.Enums.UsageMetricType metric)
        => OverageRates.FirstOrDefault(r => r.Metric.Equals(metric));
}
