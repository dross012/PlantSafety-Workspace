// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           ScrapRateCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing production Scrap Rate (scrapped
//                  units / total units) from raw quality telemetry. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes production Scrap Rate from raw quality telemetry.</summary>
public sealed class ScrapRateCalculator : IDomainService
{
    /// <summary>Computes ScrapRate = ScrappedUnits / TotalUnitsProduced (0.0 when no units were produced).</summary>
    public double Calculate(ProductionQualityInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return inputs.TotalUnitsProduced == 0
            ? 0.0
            : Math.Round(inputs.ScrappedUnits / (double)inputs.TotalUnitsProduced, 4);
    }
}
