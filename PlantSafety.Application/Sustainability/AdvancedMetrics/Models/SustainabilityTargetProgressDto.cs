// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           SustainabilityTargetProgressDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing ListSustainabilityTargetsAndProgressQuery: the Target
//                  Progress Tracker dashboard's per-target row. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>A single sustainability target and its tracked progress.</summary>
public sealed record SustainabilityTargetProgressDto
{
    /// <summary>Target identifier.</summary>
    public required Guid SustainabilityTargetId { get; init; }

    /// <summary>Owning ledger identifier.</summary>
    public required Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>The KPI this target tracks (e.g. CarbonIntensityPerUnitOutput, WasteDiversionRate).</summary>
    public required string TargetType { get; init; }

    /// <summary>Baseline value.</summary>
    public decimal BaselineValue { get; init; }

    /// <summary>Target value.</summary>
    public decimal TargetValue { get; init; }

    /// <summary>Most recently observed value.</summary>
    public decimal CurrentValue { get; init; }

    /// <summary>Date by which the target must be reached.</summary>
    public required DateTime TargetDateUtc { get; init; }

    /// <summary>Current progress status (OnTrack/AtRisk/Achieved/Missed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant of the most recent progress update, if any.</summary>
    public DateTime? LastProgressUpdateAtUtc { get; init; }
}
