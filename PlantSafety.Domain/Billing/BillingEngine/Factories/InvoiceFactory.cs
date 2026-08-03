// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Domain factory composing UsageOverageCalculator (§3.1 usage-based overage
//                  math) and InvoiceCalculationRules (subtotal/discount math) to generate a
//                  complete, internally-consistent Draft Invoice for a tenant's billing period.
//                  This is the single call site the Application layer's GenerateInvoiceCommand
//                  handler invokes -- it never re-implements any of the money math itself.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Aggregates;
using PlantSafety.Domain.Billing.BillingEngine.Rules;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Metering.Services;
using PlantSafety.Domain.Billing.Subscription.Aggregates;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.BillingEngine.Factories;

/// <summary>Domain factory generating a complete Draft <see cref="Invoice"/> from usage metering data and plan terms.</summary>
public static class InvoiceFactory
{
    /// <summary>
    /// Generates a Draft invoice for <paramref name="tenantId"/>'s <paramref name="snapshot"/>
    /// billing period, against <paramref name="plan"/>'s base price and overage rates, applying
    /// <paramref name="discountsAndCredits"/> (if any) in the order supplied.
    /// </summary>
    public static Invoice GenerateForPeriod(
        TenantId tenantId,
        SubscriptionPlan plan,
        UsageMeteringSnapshot snapshot,
        IReadOnlyList<DiscountOrCredit> discountsAndCredits,
        UtcTimestamp generatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(discountsAndCredits);

        var currency = plan.MonthlyBasePrice.Currency;
        var chargeLineItems = new List<InvoiceLineItem>
        {
            InvoiceLineItem.ForBasePlan(plan.DisplayName, plan.MonthlyBasePrice),
        };

        foreach (var overageResult in UsageOverageCalculator.ComputeAll(snapshot, plan, currency))
        {
            var rate = plan.FindOverageRate(overageResult.Metric);
            if (rate is null || overageResult.OverageQuantity <= 0)
                continue;

            var lineItem = InvoiceLineItem.ForOverage(overageResult.Metric.DisplayName, overageResult.OverageQuantity, rate.PricePerUnit, overageResult.OverageCharge);
            if (lineItem is not null)
                chargeLineItems.Add(lineItem);
        }

        var subtotal = InvoiceCalculationRules.ComputeSubtotal(chargeLineItems, currency);
        var discountResult = InvoiceCalculationRules.ApplyDiscountsAndCredits(subtotal, discountsAndCredits);

        var allLineItems = new List<InvoiceLineItem>(chargeLineItems);
        allLineItems.AddRange(discountResult.DiscountLineItems);

        return Invoice.Generate(tenantId, snapshot.Period, allLineItems, subtotal, discountResult.TotalDue, generatedAtUtc);
    }
}
