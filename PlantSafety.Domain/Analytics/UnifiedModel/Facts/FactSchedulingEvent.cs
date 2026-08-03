// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactSchedulingEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the Scheduling domain. Grain: one row per scheduled/completed
//                  event across PMScheduling, WorkOrderPriority, ShiftFatigue,
//                  EmergencyDrillPlanning, and HazardIsolationSequencing bounded contexts.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single Scheduling-domain event (PM due/completed, drill scheduled, work order
/// prioritized, isolation sequence step, shift-fatigue assessment). Grain: one row per scheduling
/// event occurrence.
/// </summary>
public sealed record FactSchedulingEvent : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimAsset</c> row, if equipment-scoped.</summary>
    public Guid? AssetKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the responsible/assigned person.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimShift</c> row, if applicable.</summary>
    public Guid? ShiftKey { get; init; }

    /// <summary>Sub-context this event originated from (PMScheduling, WorkOrderPriority, ShiftFatigue, EmergencyDrillPlanning, HazardIsolationSequencing).</summary>
    public required string SchedulingSubContext { get; init; }

    /// <summary>Event type code (e.g. "PMDue", "PMCompleted", "DrillScheduled", "IsolationStepCompleted").</summary>
    public required string EventType { get; init; }

    /// <summary>Scheduled UTC instant for the event.</summary>
    public required UtcTimestamp ScheduledAtUtc { get; init; }

    /// <summary>Actual UTC completion instant, if completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; init; }

    /// <summary>Number of minutes the event was completed late relative to schedule (0 or negative if on-time/early).</summary>
    public int VarianceMinutes { get; init; }

    /// <summary>True when the event was completed on or before its scheduled instant.</summary>
    public bool WasOnTime { get; init; }

    /// <summary>Priority code assigned at scheduling time, if applicable (e.g. WorkOrderPriority P1-P4).</summary>
    public string? PriorityCode { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "Scheduling";
}
