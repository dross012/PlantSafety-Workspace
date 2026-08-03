// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           BillingPeriod.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1/§3.3 Usage Metering / Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a single calendar-month billing period
//                  (UTC), shared by usage metering snapshots and monthly billing cycles/invoices.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.Metering.ValueObjects;

/// <summary>A single calendar-month billing period, in UTC.</summary>
public readonly record struct BillingPeriod : IComparable<BillingPeriod>
{
    /// <summary>Calendar year (UTC).</summary>
    public int Year { get; }

    /// <summary>Calendar month (1-12, UTC).</summary>
    public int Month { get; }

    /// <summary>Creates a BillingPeriod for the given year/month.</summary>
    public BillingPeriod(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), month, "BillingPeriod.Month must be between 1 and 12.");
        if (year < 2000)
            throw new ArgumentOutOfRangeException(nameof(year), year, "BillingPeriod.Year must be a plausible calendar year.");

        Year = year;
        Month = month;
    }

    /// <summary>The inclusive UTC start instant of this billing period (00:00:00 on the 1st).</summary>
    public DateTime StartUtc => new(Year, Month, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>The exclusive UTC end instant of this billing period (00:00:00 on the 1st of the following month).</summary>
    public DateTime EndUtc => StartUtc.AddMonths(1);

    /// <summary>Builds the BillingPeriod containing the given UTC instant.</summary>
    public static BillingPeriod FromUtc(DateTime instantUtc) => new(instantUtc.Year, instantUtc.Month);

    /// <summary>The billing period immediately following this one.</summary>
    public BillingPeriod Next() => FromUtc(EndUtc);

    /// <summary>The billing period immediately preceding this one.</summary>
    public BillingPeriod Previous() => FromUtc(StartUtc.AddMonths(-1));

    /// <inheritdoc/>
    public int CompareTo(BillingPeriod other) => StartUtc.CompareTo(other.StartUtc);

    /// <inheritdoc/>
    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
