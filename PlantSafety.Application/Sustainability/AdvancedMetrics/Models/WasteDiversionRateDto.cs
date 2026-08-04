// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           WasteDiversionRateDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetWasteDiversionRateQuery: the Waste Diversion Rate
//                  dashboard's rollup for a reporting period. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>Aggregated waste diversion rate for a single reporting period.</summary>
public sealed record WasteDiversionRateDto
{
    /// <summary>Source ledger identifier.</summary>
    public required Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>Total waste generated, in kilograms, across all records in the period.</summary>
    public decimal TotalWasteKg { get; init; }

    /// <summary>Total waste diverted from landfill/incineration, in kilograms, across all records.</summary>
    public decimal DivertedWasteKg { get; init; }

    /// <summary>Overall diversion rate as a percentage (0-100) for the period.</summary>
    public decimal DiversionRatePercent { get; init; }

    /// <summary>Number of individual waste diversion records contributing to this rollup.</summary>
    public int RecordCount { get; init; }
}
