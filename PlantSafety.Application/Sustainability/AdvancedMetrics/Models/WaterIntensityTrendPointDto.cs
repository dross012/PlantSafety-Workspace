// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           WaterIntensityTrendPointDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetWaterIntensityTrendQuery: one time-series point on
//                  the Energy/Water Intensity Board dashboard. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>A single water-intensity trend data point.</summary>
public sealed record WaterIntensityTrendPointDto
{
    /// <summary>Record identifier.</summary>
    public required Guid WaterIntensityMetricId { get; init; }

    /// <summary>UTC instant of the underlying meter read.</summary>
    public required DateTime ReadingAtUtc { get; init; }

    /// <summary>Water consumed, in cubic meters, over the reading interval.</summary>
    public decimal WaterConsumedCubicMeters { get; init; }

    /// <summary>Production output units the consumption is normalized against.</summary>
    public decimal OutputUnits { get; init; }

    /// <summary>Computed intensity value (WaterConsumedCubicMeters / OutputUnits).</summary>
    public decimal IntensityValue { get; init; }

    /// <summary>Calculation methodology version in force when this point was recorded.</summary>
    public required string MethodologyVersion { get; init; }
}
