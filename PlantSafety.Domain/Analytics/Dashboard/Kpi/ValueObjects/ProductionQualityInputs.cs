// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           ProductionQualityInputs.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable value objects for the Throughput, Yield, and Scrap Rate KPI
//                  calculations. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

/// <summary>Raw inputs required to compute production Throughput for a measurement period.</summary>
public sealed record ThroughputInputs
{
    /// <summary>Total number of units produced during the measurement period.</summary>
    public int UnitsProduced { get; }

    /// <summary>Elapsed wall-clock time over which the units were produced.</summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>Creates a validated ThroughputInputs value object.</summary>
    public ThroughputInputs(int unitsProduced, TimeSpan elapsedTime)
    {
        if (unitsProduced < 0)
            throw new ArgumentException("UnitsProduced must not be negative.", nameof(unitsProduced));
        if (elapsedTime <= TimeSpan.Zero)
            throw new ArgumentException("ElapsedTime must be positive.", nameof(elapsedTime));

        UnitsProduced = unitsProduced;
        ElapsedTime = elapsedTime;
    }
}

/// <summary>Raw inputs required to compute production Yield and Scrap Rate for a measurement period.</summary>
public sealed record ProductionQualityInputs
{
    /// <summary>Total number of units produced (good and defective/scrapped) during the period.</summary>
    public int TotalUnitsProduced { get; }

    /// <summary>Number of units that passed quality inspection.</summary>
    public int GoodUnitsProduced { get; }

    /// <summary>Number of units scrapped/rejected during the period.</summary>
    public int ScrappedUnits { get; }

    /// <summary>Creates a validated ProductionQualityInputs value object.</summary>
    public ProductionQualityInputs(int totalUnitsProduced, int goodUnitsProduced, int scrappedUnits)
    {
        if (totalUnitsProduced < 0)
            throw new ArgumentException("TotalUnitsProduced must not be negative.", nameof(totalUnitsProduced));
        if (goodUnitsProduced < 0 || goodUnitsProduced > totalUnitsProduced)
            throw new ArgumentException("GoodUnitsProduced must be between 0 and TotalUnitsProduced.", nameof(goodUnitsProduced));
        if (scrappedUnits < 0 || scrappedUnits > totalUnitsProduced)
            throw new ArgumentException("ScrappedUnits must be between 0 and TotalUnitsProduced.", nameof(scrappedUnits));

        TotalUnitsProduced = totalUnitsProduced;
        GoodUnitsProduced = goodUnitsProduced;
        ScrappedUnits = scrappedUnits;
    }
}
