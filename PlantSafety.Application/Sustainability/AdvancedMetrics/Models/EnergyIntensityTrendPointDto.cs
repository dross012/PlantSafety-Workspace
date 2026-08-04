// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           EnergyIntensityTrendPointDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetEnergyIntensityTrendQuery: one time-series point on
//                  the Energy Intensity Board dashboard. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>A single energy-intensity trend data point.</summary>
public sealed record EnergyIntensityTrendPointDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EnergyIntensityMetricId { get; init; }

    /// <summary>UTC instant of the underlying meter read.</summary>
    public required DateTime ReadingAtUtc { get; init; }

    /// <summary>Energy consumed, in kilowatt-hours, over the reading interval.</summary>
    public decimal EnergyConsumedKwh { get; init; }

    /// <summary>Production output units the consumption is normalized against.</summary>
    public decimal OutputUnits { get; init; }

    /// <summary>Computed intensity value (EnergyConsumedKwh / OutputUnits).</summary>
    public decimal IntensityValue { get; init; }

    /// <summary>Calculation methodology version in force when this point was recorded.</summary>
    public required string MethodologyVersion { get; init; }
}
