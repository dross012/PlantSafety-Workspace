// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           WaterIntensityMetric.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity of AdvancedMetricsLedger recording water consumption
//                  normalized against a unit of production output (water intensity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;

/// <summary>A single water-intensity meter-read record on an <see cref="Aggregates.AdvancedMetricsLedger"/>.</summary>
public sealed class WaterIntensityMetric
{
    /// <summary>Unique identity of this record.</summary>
    public WaterIntensityMetricId WaterIntensityMetricId { get; private set; }

    /// <summary>Water consumed, in cubic meters, over the reading interval.</summary>
    public decimal WaterConsumedCubicMeters { get; private set; }

    /// <summary>Production output units the consumption is normalized against.</summary>
    public decimal OutputUnits { get; private set; }

    /// <summary>Computed intensity value (WaterConsumedCubicMeters / OutputUnits).</summary>
    public decimal IntensityValue { get; private set; }

    /// <summary>Version identifier of the calculation methodology used to derive this value.</summary>
    public string MethodologyVersion { get; private set; } = string.Empty;

    /// <summary>UTC instant the meter read covers.</summary>
    public UtcTimestamp ReadingAtUtc { get; private set; }

    /// <summary>UTC instant this record was captured in the ledger.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user (or ingestion service actor) who recorded this entry.</summary>
    public UserId RecordedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private WaterIntensityMetric() { }

    /// <summary>Creates a new water intensity metric, computing intensity from consumption and output.</summary>
    public WaterIntensityMetric(
        WaterIntensityMetricId waterIntensityMetricId,
        decimal waterConsumedCubicMeters,
        decimal outputUnits,
        string methodologyVersion,
        UtcTimestamp readingAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy)
    {
        if (waterConsumedCubicMeters < 0)
            throw new ArgumentOutOfRangeException(nameof(waterConsumedCubicMeters), "Water consumed must not be negative.");
        if (outputUnits <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputUnits), "Output units must be greater than zero to compute intensity.");
        if (string.IsNullOrWhiteSpace(methodologyVersion))
            throw new ArgumentException("MethodologyVersion must not be null or whitespace.", nameof(methodologyVersion));

        WaterIntensityMetricId   = waterIntensityMetricId;
        WaterConsumedCubicMeters = waterConsumedCubicMeters;
        OutputUnits              = outputUnits;
        IntensityValue           = Math.Round(waterConsumedCubicMeters / outputUnits, 6, MidpointRounding.AwayFromZero);
        MethodologyVersion       = methodologyVersion.Trim();
        ReadingAtUtc             = readingAtUtc;
        RecordedAtUtc            = recordedAtUtc;
        RecordedBy               = recordedBy;
    }
}
