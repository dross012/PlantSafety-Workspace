// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           WorkloadTrendByShiftDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetWorkloadTrendByShiftQuery. Backs the Cognitive Load
//                  by Shift/Role dashboard. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

/// <summary>A single shift's workload-trend recalculation snapshot.</summary>
public sealed record WorkloadTrendByShiftDto
{
    /// <summary>Trend record identifier.</summary>
    public required Guid WorkloadTrendRecordId { get; init; }

    /// <summary>Owning ledger identifier.</summary>
    public required Guid CognitiveLoadLedgerId { get; init; }

    /// <summary>Shift classification.</summary>
    public required string ShiftType { get; init; }

    /// <summary>Window start.</summary>
    public DateTime PeriodStartUtc { get; init; }

    /// <summary>Window end.</summary>
    public DateTime PeriodEndUtc { get; init; }

    /// <summary>Mean cognitive-load score across the window.</summary>
    public decimal MeanCognitiveLoadScore { get; init; }

    /// <summary>Count of readings classified as overload events in the window.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>Total reading count contributing to this snapshot.</summary>
    public int ReadingCount { get; init; }

    /// <summary>UTC instant this recalculation was performed.</summary>
    public DateTime RecalculatedAtUtc { get; init; }
}
