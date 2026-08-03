// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           MtbfCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing Mean Time Between Failures (MTBF)
//                  from raw operating-time and failure-count telemetry. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes Mean Time Between Failures (MTBF) from raw reliability telemetry.</summary>
public sealed class MtbfCalculator : IDomainService
{
    /// <summary>
    /// Computes MTBF = TotalOperatingTime / FailureCount. Returns <see cref="TimeSpan.Zero"/>
    /// when no failures were recorded (undefined MTBF, treated conservatively as zero uptime margin).
    /// </summary>
    public TimeSpan Calculate(MtbfInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return inputs.FailureCount == 0
            ? TimeSpan.Zero
            : TimeSpan.FromTicks(inputs.TotalOperatingTime.Ticks / inputs.FailureCount);
    }
}
