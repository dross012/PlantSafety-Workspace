// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactMaintenanceWorkOrder.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the Maintenance domain. Grain: one row per preventive or
//                  predictive maintenance work order, sourced from
//                  PlantSafety.Domain.Operational.PreventiveMaintenance and .PredictiveMaintenance.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single maintenance work order (preventive or predictive). Grain: one row per
/// work order lifecycle terminal state (created/scheduled/completed).
/// </summary>
public sealed record FactMaintenanceWorkOrder : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimAsset</c> row for the maintained equipment.</summary>
    public required Guid AssetKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the assigned technician.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Maintenance strategy: "Preventive" or "Predictive".</summary>
    public required string MaintenanceType { get; init; }

    /// <summary>Work order lifecycle status at capture time (e.g. "Scheduled", "InProgress", "Completed", "Overdue").</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the work order was scheduled/due.</summary>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <summary>UTC instant the work order was completed, if completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; init; }

    /// <summary>Downtime, in minutes, attributed to this work order.</summary>
    public int DowntimeMinutes { get; init; }

    /// <summary>Predicted failure probability at the time of capture, for predictive-maintenance rows (0.0-1.0).</summary>
    public double? PredictedFailureProbability { get; init; }

    /// <summary>True when the work order was completed within its scheduled window (no overdue variance).</summary>
    public bool WasOnSchedule { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "Maintenance";
}
