// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceCalculationRules.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Pure, side-effect-free calculation rules for Invoice generation: subtotal
//                  computation from charge line items (BasePlan + Overage) and sequential
//                  discount/credit application. This is the highest-risk-of-being-wrong logic in
//                  the Billing module (per the task's own scope note) -- deliberately isolated
//                  here as pure static functions with no aggregate/persistence dependency, so it
//                  is directly and exhaustively unit-testable. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.BillingEngine.Rules;

/// <summary>The outcome of applying a sequence of discounts/credits to a pre-discount subtotal.</summary>
/// <param name="DiscountLineItems">One line item per applied discount/credit, in application order.</param>
/// <param name="TotalDue">The final amount due after every discount/credit has been applied, never negative.</param>
public sealed record DiscountApplicationResult(IReadOnlyList<InvoiceLineItem> DiscountLineItems, Money TotalDue);

/// <summary>Pure calculation rules for Invoice subtotal/discount/total-due computation.</summary>
public static class InvoiceCalculationRules
{
    /// <summary>
    /// Sums the <see cref="InvoiceLineItem.Amount"/> of every charge line item (BasePlan + Overage)
    /// to produce the pre-discount subtotal.
    /// </summary>
    public static Money ComputeSubtotal(IReadOnlyList<InvoiceLineItem> chargeLineItems, string currency)
    {
        ArgumentNullException.ThrowIfNull(chargeLineItems);
        return chargeLineItems.Aggregate(Money.Zero(currency), (running, item) => running + item.Amount);
    }

    /// <summary>
    /// Applies each discount/credit in <paramref name="discountsAndCredits"/> sequentially against
    /// the running subtotal (each discount/credit is computed against the balance remaining AFTER
    /// every prior discount/credit in the list has already been applied), producing one line item
    /// per discount/credit and the final total due. The running total is clamped so it can never go
    /// negative -- an invoice is never a net credit to the tenant through this path.
    /// </summary>
    public static DiscountApplicationResult ApplyDiscountsAndCredits(Money subtotal, IReadOnlyList<DiscountOrCredit> discountsAndCredits)
    {
        ArgumentNullException.ThrowIfNull(discountsAndCredits);

        var lineItems = new List<InvoiceLineItem>(discountsAndCredits.Count);
        var running = subtotal;

        foreach (var discountOrCredit in discountsAndCredits)
        {
            var lineItem = InvoiceLineItem.ForDiscountOrCredit(discountOrCredit, running);
            lineItems.Add(lineItem);
            running = (running + lineItem.Amount).ClampToZero();
        }

        return new DiscountApplicationResult(lineItems, running);
    }
}
