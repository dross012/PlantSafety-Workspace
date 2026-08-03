// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Subscription
//  Layer:          Domain
//  File:           OverageRate.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.2/§3.3 Subscription Plans / Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object pairing a metered usage dimension with the per-unit
//                  price a tenant is charged once usage exceeds that dimension's plan limit.
//                  Only metrics with a finite (non-null) plan limit can meaningfully overage-price
//                  -- ApiCalls/ModuleUsage do not carry a numeric cap in this catalog (see
//                  SubscriptionPlanCatalog remarks) and are therefore never billed for overage.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.Subscription.ValueObjects;

/// <summary>Per-unit overage price for a single metered usage dimension.</summary>
/// <param name="Metric">The usage dimension this rate applies to.</param>
/// <param name="PricePerUnit">The price charged per unit of usage beyond the plan's limit for <paramref name="Metric"/>.</param>
/// <param name="UnitSize">
/// The size of one billable unit for <paramref name="Metric"/> (e.g. 1 for whole-unit metrics like
/// UserCount/ScheduledTasks, or 1_073_741_824 for StorageBytes priced per GiB).
/// </param>
public sealed record OverageRate(UsageMetricType Metric, Money PricePerUnit, decimal UnitSize)
{
    /// <summary>Computes the overage charge for the given quantity of usage beyond the plan limit (already non-negative).</summary>
    public Money ComputeCharge(decimal overageQuantity)
    {
        if (overageQuantity <= 0)
            return Money.Zero(PricePerUnit.Currency);

        var billableUnits = Math.Ceiling(overageQuantity / UnitSize);
        return PricePerUnit * billableUnits;
    }
}
