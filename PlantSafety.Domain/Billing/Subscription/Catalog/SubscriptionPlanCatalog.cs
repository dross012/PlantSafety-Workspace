// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           SubscriptionPlanCatalog.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
//  SchemaVersion:  2026.1
//  Description:    Closed, in-process reference-data catalog of every subscription plan the
//                  platform currently offers, keyed by the SAME Code values as the pre-existing
//                  Tenancy.Tenant.ValueObjects.SubscriptionPlanCode (Trial/Standard/Professional/
//                  Enterprise), plus a fifth Custom entry for negotiated contracts. This is the
//                  "authoritative plan catalog" SubscriptionPlanCode's own header comment
//                  anticipated Billing would own -- Tenant.PlanCode.Code is the unchanged FK into
//                  this catalog (see SubscriptionPlanCatalog.Resolve). Modeled as static reference
//                  data (not a DB table) for the same reason SubscriptionPlanCode itself is: a
//                  small, closed, rarely-changing set that every environment must agree on
//                  identically, with no seed-migration drift risk. A future pass MAY promote this
//                  to a DB-backed catalog (e.g. for tenant-negotiated Custom plan terms) without
//                  breaking any caller of <see cref="Resolve(PlanTier)"/> -- additive evolution.
// ============================================================================
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Subscription.Aggregates;
using PlantSafety.Domain.Billing.Subscription.Enums;
using PlantSafety.Domain.Billing.Subscription.ValueObjects;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.Subscription.Catalog;

/// <summary>Closed, in-process reference-data catalog of every subscription plan currently offered by the platform.</summary>
public static class SubscriptionPlanCatalog
{
    private const string DefaultCurrency = "USD";

    /// <summary>Time-limited evaluation plan: 5 users, 60 req/min, 1 GiB storage, restrictive module set, no overage billing (Trial tenants cannot exceed their caps).</summary>
    public static readonly SubscriptionPlan Trial = SubscriptionPlan.Create(
        PlanTier.Trial, "Trial", Money.Zero(DefaultCurrency),
        new PlanLimits(MaxUsers: 5, ApiRateLimitPerMinute: 60, StorageQuotaBytes: 1L * 1024 * 1024 * 1024, AllowedModuleCodes: ["Alerts", "Checklists", "Auditing"]),
        overageRates: [], isActive: true);

    /// <summary>Standard commercial plan: 25 users, 300 req/min, 25 GiB storage, mid-tier module set, moderate overage pricing.</summary>
    public static readonly SubscriptionPlan Standard = SubscriptionPlan.Create(
        PlanTier.Standard, "Standard", Money.From(499m, DefaultCurrency),
        new PlanLimits(MaxUsers: 25, ApiRateLimitPerMinute: 300, StorageQuotaBytes: 25L * 1024 * 1024 * 1024,
            AllowedModuleCodes: ["Alerts", "Checklists", "Auditing", "Compliance", "Training", "Permits", "RiskManagement", "Scheduling", "PPE"],
            MaxScheduledTasksPerMonth: 500, MaxEventVolumePerMonth: 100_000),
        overageRates:
        [
            new OverageRate(UsageMetricType.UserCount, Money.From(15m, DefaultCurrency), UnitSize: 1m),
            new OverageRate(UsageMetricType.ScheduledTasks, Money.From(0.02m, DefaultCurrency), UnitSize: 1m),
            new OverageRate(UsageMetricType.StorageBytes, Money.From(5m, DefaultCurrency), UnitSize: 1024m * 1024 * 1024),
            new OverageRate(UsageMetricType.EventVolume, Money.From(0.001m, DefaultCurrency), UnitSize: 1m),
        ],
        isActive: true);

    /// <summary>Professional commercial plan: 100 users, 1000 req/min, 250 GiB storage, expanded module access, lower per-unit overage pricing.</summary>
    public static readonly SubscriptionPlan Professional = SubscriptionPlan.Create(
        PlanTier.Professional, "Professional", Money.From(1999m, DefaultCurrency),
        new PlanLimits(MaxUsers: 100, ApiRateLimitPerMinute: 1000, StorageQuotaBytes: 250L * 1024 * 1024 * 1024, AllowedModuleCodes: null,
            MaxScheduledTasksPerMonth: 5000, MaxEventVolumePerMonth: 2_000_000),
        overageRates:
        [
            new OverageRate(UsageMetricType.UserCount, Money.From(10m, DefaultCurrency), UnitSize: 1m),
            new OverageRate(UsageMetricType.ScheduledTasks, Money.From(0.01m, DefaultCurrency), UnitSize: 1m),
            new OverageRate(UsageMetricType.StorageBytes, Money.From(3m, DefaultCurrency), UnitSize: 1024m * 1024 * 1024),
            new OverageRate(UsageMetricType.EventVolume, Money.From(0.0005m, DefaultCurrency), UnitSize: 1m),
        ],
        isActive: true);

    /// <summary>Enterprise commercial plan: unlimited users/storage, highest API rate limit, full module access, lowest per-unit overage pricing (storage/events only -- users/modules are unlimited so never overage-billed).</summary>
    public static readonly SubscriptionPlan Enterprise = SubscriptionPlan.Create(
        PlanTier.Enterprise, "Enterprise", Money.From(7999m, DefaultCurrency),
        new PlanLimits(MaxUsers: null, ApiRateLimitPerMinute: 5000, StorageQuotaBytes: null, AllowedModuleCodes: null,
            MaxScheduledTasksPerMonth: null, MaxEventVolumePerMonth: 50_000_000),
        overageRates:
        [
            new OverageRate(UsageMetricType.EventVolume, Money.From(0.0002m, DefaultCurrency), UnitSize: 1m),
        ],
        isActive: true);

    /// <summary>Negotiated/custom-contract plan placeholder: unlimited everything, no default overage pricing (negotiated per contract). Not self-service-selectable -- see PlanTier.Custom remarks.</summary>
    public static readonly SubscriptionPlan Custom = SubscriptionPlan.Create(
        PlanTier.Custom, "Custom", Money.Zero(DefaultCurrency),
        new PlanLimits(MaxUsers: null, ApiRateLimitPerMinute: 10000, StorageQuotaBytes: null, AllowedModuleCodes: null),
        overageRates: [], isActive: false);

    /// <summary>Every plan currently defined in the catalog.</summary>
    public static readonly IReadOnlyList<SubscriptionPlan> All = [Trial, Standard, Professional, Enterprise, Custom];

    /// <summary>Resolves the SubscriptionPlan reference-data entry for the given <see cref="PlanTier"/>.</summary>
    public static SubscriptionPlan Resolve(PlanTier tier)
        => All.FirstOrDefault(p => p.Tier.Equals(tier))
           ?? throw new ArgumentException($"No SubscriptionPlan catalog entry for PlanTier '{tier}'.", nameof(tier));

    /// <summary>
    /// Resolves the SubscriptionPlan reference-data entry for the given Tenant-carried
    /// <see cref="SubscriptionPlanCode"/> -- the additive bridge from the pre-existing Tenant
    /// aggregate's plan reference into this catalog's richer commercial-terms model. Every
    /// existing SubscriptionPlanCode value (Trial/Standard/Professional/Enterprise) has a matching
    /// catalog entry by construction (identical Code strings), so this never throws for any value
    /// a Tenant can actually carry today.
    /// </summary>
    public static SubscriptionPlan Resolve(SubscriptionPlanCode planCode)
        => Resolve(PlanTier.From(planCode.Code));
}
