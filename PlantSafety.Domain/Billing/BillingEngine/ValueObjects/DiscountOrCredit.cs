// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           DiscountOrCredit.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing either a percentage/fixed-amount discount
//                  or an account credit applied against an invoice's pre-discount subtotal.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Billing.BillingEngine.ValueObjects;

/// <summary>Distinguishes a percentage discount, a fixed-amount discount, and an account credit.</summary>
public enum DiscountOrCreditKind
{
    /// <summary>A percentage-of-subtotal discount (e.g. 10% off).</summary>
    PercentageDiscount = 0,

    /// <summary>A fixed monetary discount (e.g. $50 off).</summary>
    FixedAmountDiscount = 1,

    /// <summary>A fixed monetary account credit (e.g. goodwill credit, prepaid balance).</summary>
    Credit = 2,
}

/// <summary>A discount or credit applied against an invoice's pre-discount subtotal.</summary>
/// <param name="Kind">Whether this is a percentage discount, fixed discount, or credit.</param>
/// <param name="Description">Human-readable description shown on the invoice.</param>
/// <param name="PercentageValue">
/// The discount percentage (0-100), used only when <paramref name="Kind"/> is
/// <see cref="DiscountOrCreditKind.PercentageDiscount"/>; otherwise <see langword="null"/>.
/// </param>
/// <param name="FixedAmount">
/// The fixed monetary reduction, used only when <paramref name="Kind"/> is
/// <see cref="DiscountOrCreditKind.FixedAmountDiscount"/> or <see cref="DiscountOrCreditKind.Credit"/>;
/// otherwise <see langword="null"/>.
/// </param>
public sealed record DiscountOrCredit(DiscountOrCreditKind Kind, string Description, decimal? PercentageValue, Money? FixedAmount)
{
    /// <summary>Creates a percentage-of-subtotal discount.</summary>
    public static DiscountOrCredit Percentage(string description, decimal percentage)
    {
        if (percentage is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "DiscountOrCredit percentage must be between 0 (exclusive) and 100 (inclusive).");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("DiscountOrCredit.Description must not be empty.", nameof(description));

        return new DiscountOrCredit(DiscountOrCreditKind.PercentageDiscount, description.Trim(), percentage, null);
    }

    /// <summary>Creates a fixed-amount discount.</summary>
    public static DiscountOrCredit FixedDiscount(string description, Money amount)
    {
        if (amount.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "DiscountOrCredit fixed discount amount must be positive.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("DiscountOrCredit.Description must not be empty.", nameof(description));

        return new DiscountOrCredit(DiscountOrCreditKind.FixedAmountDiscount, description.Trim(), null, amount);
    }

    /// <summary>Creates a fixed-amount account credit.</summary>
    public static DiscountOrCredit AccountCredit(string description, Money amount)
    {
        if (amount.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "DiscountOrCredit credit amount must be positive.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("DiscountOrCredit.Description must not be empty.", nameof(description));

        return new DiscountOrCredit(DiscountOrCreditKind.Credit, description.Trim(), null, amount);
    }

    /// <summary>
    /// Computes the monetary reduction this discount/credit applies against
    /// <paramref name="subtotalBeforeThisItem"/>, clamped so it can never reduce the running total
    /// below zero (a discount/credit larger than the remaining subtotal is capped at the subtotal).
    /// </summary>
    public Money ComputeReduction(Money subtotalBeforeThisItem)
    {
        var rawReduction = Kind switch
        {
            DiscountOrCreditKind.PercentageDiscount => subtotalBeforeThisItem * (PercentageValue!.Value / 100m),
            DiscountOrCreditKind.FixedAmountDiscount => FixedAmount!.Value,
            DiscountOrCreditKind.Credit => FixedAmount!.Value,
            _ => throw new InvalidOperationException($"Unhandled DiscountOrCreditKind '{Kind}'."),
        };

        return rawReduction > subtotalBeforeThisItem ? subtotalBeforeThisItem : rawReduction;
    }
}
