// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           YieldCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing production Yield (good units /
//                  total units) from raw quality telemetry. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes production Yield from raw quality telemetry.</summary>
public sealed class YieldCalculator : IDomainService
{
    /// <summary>Computes Yield = GoodUnitsProduced / TotalUnitsProduced (0.0 when no units were produced).</summary>
    public double Calculate(ProductionQualityInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return inputs.TotalUnitsProduced == 0
            ? 0.0
            : Math.Round(inputs.GoodUnitsProduced / (double)inputs.TotalUnitsProduced, 4);
    }
}
