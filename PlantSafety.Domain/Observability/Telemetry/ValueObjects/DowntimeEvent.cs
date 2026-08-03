// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           DowntimeEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable capture of an unplanned or planned machine downtime interval,
//                  feeding the OEE availability factor and MTBF/MTTR reliability KPIs.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Telemetry.ValueObjects;

/// <summary>A recorded machine downtime interval with an associated reason code.</summary>
public sealed record DowntimeEvent
{
    /// <summary>Identity of the affected machine.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Stable machine-readable downtime reason code (e.g. "JAM", "PLANNED_PM", "MATERIAL_STARVED").</summary>
    public required string ReasonCode { get; init; }

    /// <summary>UTC instant the downtime began.</summary>
    public required UtcTimestamp StartUtc { get; init; }

    /// <summary>UTC instant the downtime ended, or null when still ongoing.</summary>
    public UtcTimestamp? EndUtc { get; init; }

    /// <summary>True when the downtime event was pre-scheduled (e.g. planned PM) rather than a breakdown.</summary>
    public bool IsPlanned { get; init; }

    /// <summary>Elapsed downtime duration; zero when the event has not yet ended.</summary>
    public TimeSpan Duration => EndUtc is { } end ? end.Value - StartUtc.Value : TimeSpan.Zero;
}
