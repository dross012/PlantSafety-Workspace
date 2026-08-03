// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           ReliabilityInputs.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable value objects for equipment reliability KPIs: Mean Time Between
//                  Failures (MTBF) and Mean Time To Repair (MTTR). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

/// <summary>Raw inputs required to compute Mean Time Between Failures for an asset/period.</summary>
public sealed record MtbfInputs
{
    /// <summary>Total time the asset was in operation (uptime) during the measurement period.</summary>
    public TimeSpan TotalOperatingTime { get; }

    /// <summary>Number of discrete failure events recorded during the measurement period.</summary>
    public int FailureCount { get; }

    /// <summary>Creates a validated MtbfInputs value object.</summary>
    public MtbfInputs(TimeSpan totalOperatingTime, int failureCount)
    {
        if (totalOperatingTime < TimeSpan.Zero)
            throw new ArgumentException("TotalOperatingTime must not be negative.", nameof(totalOperatingTime));
        if (failureCount < 0)
            throw new ArgumentException("FailureCount must not be negative.", nameof(failureCount));

        TotalOperatingTime = totalOperatingTime;
        FailureCount = failureCount;
    }
}

/// <summary>Raw inputs required to compute Mean Time To Repair for an asset/period.</summary>
public sealed record MttrInputs
{
    /// <summary>Total time spent under active repair across all repair events in the period.</summary>
    public TimeSpan TotalRepairTime { get; }

    /// <summary>Number of discrete repair events recorded during the measurement period.</summary>
    public int RepairCount { get; }

    /// <summary>Creates a validated MttrInputs value object.</summary>
    public MttrInputs(TimeSpan totalRepairTime, int repairCount)
    {
        if (totalRepairTime < TimeSpan.Zero)
            throw new ArgumentException("TotalRepairTime must not be negative.", nameof(totalRepairTime));
        if (repairCount < 0)
            throw new ArgumentException("RepairCount must not be negative.", nameof(repairCount));

        TotalRepairTime = totalRepairTime;
        RepairCount = repairCount;
    }
}
