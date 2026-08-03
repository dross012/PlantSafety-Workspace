// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Kpi
//  Layer:          Domain
//  File:           OeeCalculator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Unit-testable domain service computing Overall Equipment Effectiveness
//                  (OEE) from raw production telemetry, per the canonical
//                  Availability x Performance x Quality formula. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;

/// <summary>Computes Overall Equipment Effectiveness (OEE) from raw production telemetry.</summary>
public sealed class OeeCalculator : IDomainService
{
    /// <summary>Computes the OEE result for the supplied <paramref name="inputs"/>.</summary>
    public OeeResult Calculate(OeeInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var runTime = inputs.PlannedProductionTime - inputs.DowntimeDuration;

        var availability = runTime <= TimeSpan.Zero
            ? 0.0
            : runTime.Ticks / (double)inputs.PlannedProductionTime.Ticks;

        var performance = runTime <= TimeSpan.Zero || inputs.TotalUnitsProduced == 0
            ? 0.0
            : Math.Clamp(
                (inputs.IdealCycleTime.Ticks * inputs.TotalUnitsProduced) / (double)runTime.Ticks, 0.0, 1.0);

        var quality = inputs.TotalUnitsProduced == 0
            ? 0.0
            : inputs.GoodUnitsProduced / (double)inputs.TotalUnitsProduced;

        return new OeeResult(
            Availability: Math.Round(Math.Clamp(availability, 0.0, 1.0), 4),
            Performance: Math.Round(performance, 4),
            Quality: Math.Round(Math.Clamp(quality, 0.0, 1.0), 4));
    }
}
