// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           OeeInputs.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable value objects for the Overall Equipment Effectiveness (OEE)
//                  KPI calculation: Availability x Performance x Quality. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

/// <summary>Raw inputs required to compute Overall Equipment Effectiveness for a production run.</summary>
public sealed record OeeInputs
{
    /// <summary>Total scheduled production time for the measurement period.</summary>
    public TimeSpan PlannedProductionTime { get; }

    /// <summary>Unplanned downtime subtracted from the planned production time.</summary>
    public TimeSpan DowntimeDuration { get; }

    /// <summary>Ideal (nameplate/rated) cycle time to produce a single unit.</summary>
    public TimeSpan IdealCycleTime { get; }

    /// <summary>Total number of units produced (good and defective) during the period.</summary>
    public int TotalUnitsProduced { get; }

    /// <summary>Number of units produced that passed quality inspection.</summary>
    public int GoodUnitsProduced { get; }

    /// <summary>Creates a validated OeeInputs value object.</summary>
    public OeeInputs(
        TimeSpan plannedProductionTime,
        TimeSpan downtimeDuration,
        TimeSpan idealCycleTime,
        int totalUnitsProduced,
        int goodUnitsProduced)
    {
        if (plannedProductionTime <= TimeSpan.Zero)
            throw new ArgumentException("PlannedProductionTime must be positive.", nameof(plannedProductionTime));
        if (downtimeDuration < TimeSpan.Zero)
            throw new ArgumentException("DowntimeDuration must not be negative.", nameof(downtimeDuration));
        if (downtimeDuration > plannedProductionTime)
            throw new ArgumentException("DowntimeDuration must not exceed PlannedProductionTime.", nameof(downtimeDuration));
        if (idealCycleTime <= TimeSpan.Zero)
            throw new ArgumentException("IdealCycleTime must be positive.", nameof(idealCycleTime));
        if (totalUnitsProduced < 0)
            throw new ArgumentException("TotalUnitsProduced must not be negative.", nameof(totalUnitsProduced));
        if (goodUnitsProduced < 0 || goodUnitsProduced > totalUnitsProduced)
            throw new ArgumentException("GoodUnitsProduced must be between 0 and TotalUnitsProduced.", nameof(goodUnitsProduced));

        PlannedProductionTime = plannedProductionTime;
        DowntimeDuration = downtimeDuration;
        IdealCycleTime = idealCycleTime;
        TotalUnitsProduced = totalUnitsProduced;
        GoodUnitsProduced = goodUnitsProduced;
    }
}

/// <summary>Computed Overall Equipment Effectiveness result and its three constituent factors.</summary>
public sealed record OeeResult(double Availability, double Performance, double Quality)
{
    /// <summary>Overall Equipment Effectiveness = Availability x Performance x Quality.</summary>
    public double Oee => Math.Round(Availability * Performance * Quality, 4);
}
