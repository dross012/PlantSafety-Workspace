// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           CycleTimeReading.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable capture of an observed machine cycle time against its rated
//                  ideal cycle time, used by the OEE performance factor and the telemetry
//                  anomaly detector. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Telemetry.ValueObjects;

/// <summary>An observed machine cycle time compared against the rated ideal cycle time.</summary>
public sealed record CycleTimeReading
{
    /// <summary>Identity of the monitored machine.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Actual observed cycle time for the most recently completed unit.</summary>
    public required TimeSpan ObservedCycleTime { get; init; }

    /// <summary>Rated/nameplate ideal cycle time for this machine.</summary>
    public required TimeSpan IdealCycleTime { get; init; }

    /// <summary>UTC instant the cycle completed.</summary>
    public required UtcTimestamp ObservedAtUtc { get; init; }

    /// <summary>Ratio of observed to ideal cycle time; values above 1.0 indicate the machine is running slower than rated.</summary>
    public double CycleTimeRatio => IdealCycleTime > TimeSpan.Zero
        ? ObservedCycleTime.Ticks / (double)IdealCycleTime.Ticks
        : 0.0;
}
