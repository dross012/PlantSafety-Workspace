// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           QualityMetricReading.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable capture of a defect observation for a monitored machine/process,
//                  feeding quality drift detection and the Yield/ScrapRate KPI calculators.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Telemetry.ValueObjects;

/// <summary>A single defect/quality observation recorded for a monitored machine or process.</summary>
public sealed record QualityMetricReading
{
    /// <summary>Identity of the monitored machine/process.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Stable machine-readable defect code (e.g. "DIM_OUT_OF_SPEC", "SURFACE_DEFECT").</summary>
    public required string DefectCode { get; init; }

    /// <summary>Count of defective units observed with this defect code.</summary>
    public required int DefectCount { get; init; }

    /// <summary>UTC instant the observation was recorded.</summary>
    public required UtcTimestamp ObservedAtUtc { get; init; }
}
