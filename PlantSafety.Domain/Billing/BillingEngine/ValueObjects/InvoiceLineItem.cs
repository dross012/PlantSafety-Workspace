// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceLineItem.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a single line item on an Invoice: base
//                  plan charge, a single metric's overage charge, or a discount/credit reduction.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Enums;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.BillingEngine.ValueObjects;

/// <summary>A single line item on an <see cref="Aggregates.Invoice"/>.</summary>
/// <param name="Type">Classification of this line item.</param>
/// <param name="Description">Human-readable description (e.g. <c>"Standard plan -- monthly base"</c>, <c>"UserCount overage: 12 units @ $15.00"</c>).</param>
/// <param name="Quantity">The billable quantity this line item represents (1 for BasePlan/Discount/Credit; the overage quantity for Overage).</param>
/// <param name="UnitPrice">Price per unit of <paramref name="Quantity"/>.</param>
/// <param name="Amount">
/// The total amount this line item contributes to the invoice subtotal. Positive for
/// BasePlan/Overage; negative (or zero) for Discount/Credit.
/// </param>
public sealed record InvoiceLineItem(LineItemType Type, string Description, decimal Quantity, Money UnitPrice, Money Amount)
{
    /// <summary>Builds a BasePlan line item for the plan's recurring monthly charge.</summary>
    public static InvoiceLineItem ForBasePlan(string planDisplayName, Money monthlyBasePrice)
        => new(LineItemType.BasePlan, $"{planDisplayName} plan -- monthly base charge", 1m, monthlyBasePrice, monthlyBasePrice);

    /// <summary>Builds an Overage line item for a single metric's overage charge. Returns <see langword="null"/> if there is no billable overage.</summary>
    public static InvoiceLineItem? ForOverage(string metricDisplayName, decimal overageQuantity, Money unitPrice, Money overageCharge)
    {
        if (overageQuantity <= 0 || overageCharge.Amount <= 0)
            return null;

        return new InvoiceLineItem(
            LineItemType.Overage,
            $"{metricDisplayName} overage: {overageQuantity:0.##} unit(s) @ {unitPrice}",
            overageQuantity, unitPrice, overageCharge);
    }

    /// <summary>Builds a Discount/Credit line item from a <see cref="DiscountOrCredit"/> applied against the given pre-discount subtotal.</summary>
    public static InvoiceLineItem ForDiscountOrCredit(DiscountOrCredit discountOrCredit, Money subtotalBeforeThisItem)
    {
        ArgumentNullException.ThrowIfNull(discountOrCredit);

        var amount = discountOrCredit.ComputeReduction(subtotalBeforeThisItem);
        var type = discountOrCredit.Kind == DiscountOrCreditKind.Credit ? LineItemType.Credit : LineItemType.Discount;

        return new InvoiceLineItem(type, discountOrCredit.Description, 1m, amount, new Money(-amount.Amount, amount.Currency));
    }
}
