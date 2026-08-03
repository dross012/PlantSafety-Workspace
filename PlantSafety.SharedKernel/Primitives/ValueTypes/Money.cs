/*==========================================================================================================================
    AUTHOR:             PlantSafety Platform Team (Phase 7 §3 Billing, Metering & Subscription Management)
    CREATED:            2026-08-03
    FILE:               Money.cs
    DESCRIPTION:        Canonical immutable value type representing a monetary amount in a fixed currency.
                        Introduced for the Billing bounded context (SubscriptionPlan pricing, Invoice line items,
                        discounts/credits, payment processor charge/refund amounts) but placed in SharedKernel
                        because "an amount of money in a currency" is a cross-cutting primitive, not something
                        specific to Billing -- any future module needing a monetary value should reuse this type
                        rather than re-inventing a decimal+string pair. Uses banker's-rounding-free arithmetic
                        (2 decimal places, MidpointRounding.AwayFromZero) so overage/discount math is deterministic
                        and auditable. Only same-currency arithmetic is permitted; cross-currency operations throw.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Immutable value type representing a monetary amount denominated in a single ISO-4217 currency
/// code. All arithmetic operators require both operands to share the same <see cref="Currency"/>
/// and throw <see cref="InvalidOperationException"/> otherwise -- there is no implicit currency
/// conversion anywhere in this type.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    /// <summary>The monetary amount, always rounded to 2 decimal places.</summary>
    public decimal Amount { get; }

    /// <summary>ISO-4217 currency code (e.g. <c>"USD"</c>), always upper-cased.</summary>
    public string Currency { get; }

    /// <summary>Creates a Money value, rounding <paramref name="amount"/> to 2 decimal places.</summary>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("Money.Currency must be a 3-letter ISO-4217 code.", nameof(currency));

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    /// <summary>Zero amount in the given currency.</summary>
    public static Money Zero(string currency) => new(0m, currency);

    /// <summary>Creates a Money value from a decimal amount and currency code.</summary>
    public static Money From(decimal amount, string currency) => new(amount, currency);

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Cannot operate on Money values in different currencies ('{left.Currency}' vs '{right.Currency}').");
    }

    /// <summary>Adds two Money values of the same currency.</summary>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>Subtracts two Money values of the same currency.</summary>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>Multiplies a Money value by a scalar quantity (e.g. unit price × quantity).</summary>
    public static Money operator *(Money money, decimal quantity) => new(money.Amount * quantity, money.Currency);

    /// <summary>Returns <see langword="true"/> when <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    public static bool operator >=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    public static bool operator <=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    /// <inheritdoc/>
    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    /// <summary>Returns a copy of this Money value clamped to a minimum of zero (never negative).</summary>
    public Money ClampToZero() => Amount < 0 ? Zero(Currency) : this;

    /// <inheritdoc/>
    public override string ToString() => $"{Amount:0.00} {Currency}";
}
