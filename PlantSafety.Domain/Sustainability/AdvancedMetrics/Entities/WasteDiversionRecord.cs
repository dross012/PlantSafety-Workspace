// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           WasteDiversionRecord.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity of AdvancedMetricsLedger recording total waste generated
//                  vs. waste diverted from landfill/incineration (recycled, composted, reused, etc.).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;

/// <summary>A single waste-diversion record on an <see cref="Aggregates.AdvancedMetricsLedger"/>.</summary>
public sealed class WasteDiversionRecord
{
    /// <summary>Unique identity of this record.</summary>
    public WasteDiversionRecordId WasteDiversionRecordId { get; private set; }

    /// <summary>Total waste generated, in kilograms, over the reporting interval.</summary>
    public decimal TotalWasteKg { get; private set; }

    /// <summary>Waste diverted from landfill/incineration (recycled, composted, reused), in kilograms.</summary>
    public decimal DivertedWasteKg { get; private set; }

    /// <summary>Computed diversion rate as a percentage (0-100).</summary>
    public decimal DiversionRatePercent { get; private set; }

    /// <summary>Version identifier of the calculation methodology used to derive this value.</summary>
    public string MethodologyVersion { get; private set; } = string.Empty;

    /// <summary>UTC instant the interval covers.</summary>
    public UtcTimestamp IntervalEndAtUtc { get; private set; }

    /// <summary>UTC instant this record was captured in the ledger.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded this entry.</summary>
    public UserId RecordedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private WasteDiversionRecord() { }

    /// <summary>Creates a new waste diversion record, computing the diversion rate from the raw totals.</summary>
    public WasteDiversionRecord(
        WasteDiversionRecordId wasteDiversionRecordId,
        decimal totalWasteKg,
        decimal divertedWasteKg,
        string methodologyVersion,
        UtcTimestamp intervalEndAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy)
    {
        if (totalWasteKg < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWasteKg), "Total waste must not be negative.");
        if (divertedWasteKg < 0 || divertedWasteKg > totalWasteKg)
            throw new ArgumentOutOfRangeException(nameof(divertedWasteKg), "Diverted waste must be between zero and total waste.");
        if (string.IsNullOrWhiteSpace(methodologyVersion))
            throw new ArgumentException("MethodologyVersion must not be null or whitespace.", nameof(methodologyVersion));

        WasteDiversionRecordId = wasteDiversionRecordId;
        TotalWasteKg           = totalWasteKg;
        DivertedWasteKg        = divertedWasteKg;
        DiversionRatePercent   = totalWasteKg == 0
            ? 0m
            : Math.Round(divertedWasteKg / totalWasteKg * 100m, 2, MidpointRounding.AwayFromZero);
        MethodologyVersion     = methodologyVersion.Trim();
        IntervalEndAtUtc       = intervalEndAtUtc;
        RecordedAtUtc          = recordedAtUtc;
        RecordedBy             = recordedBy;
    }
}
