// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           MttrCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing Mean Time To Repair (MTTR) from
//                  raw repair-time and repair-count telemetry. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes Mean Time To Repair (MTTR) from raw reliability telemetry.</summary>
public sealed class MttrCalculator : IDomainService
{
    /// <summary>
    /// Computes MTTR = TotalRepairTime / RepairCount. Returns <see cref="TimeSpan.Zero"/>
    /// when no repairs were recorded.
    /// </summary>
    public TimeSpan Calculate(MttrInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return inputs.RepairCount == 0
            ? TimeSpan.Zero
            : TimeSpan.FromTicks(inputs.TotalRepairTime.Ticks / inputs.RepairCount);
    }
}
