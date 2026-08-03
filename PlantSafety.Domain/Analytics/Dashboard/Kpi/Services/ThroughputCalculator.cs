// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           ThroughputCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing production throughput (units per
//                  hour) from raw production-count and elapsed-time telemetry. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes production throughput (units/hour) from raw production telemetry.</summary>
public sealed class ThroughputCalculator : IDomainService
{
    /// <summary>Computes throughput in units per hour = UnitsProduced / ElapsedTime.TotalHours.</summary>
    public double CalculateUnitsPerHour(ThroughputInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return Math.Round(inputs.UnitsProduced / inputs.ElapsedTime.TotalHours, 4);
    }
}
