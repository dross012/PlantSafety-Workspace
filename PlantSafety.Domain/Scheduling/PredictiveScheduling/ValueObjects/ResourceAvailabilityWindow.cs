// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ResourceAvailabilityWindow.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object describing a contiguous window during which a
//                  scheduling resource (crew or machine) has a given amount of spare
//                  capacity. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

/// <summary>A contiguous UTC time window during which a resource has available capacity.</summary>
public sealed record ResourceAvailabilityWindow
{
    /// <summary>Identity of the resource this window belongs to.</summary>
    public SchedulingResourceId ResourceId { get; }

    /// <summary>UTC start of the availability window.</summary>
    public UtcTimestamp StartUtc { get; }

    /// <summary>UTC end of the availability window.</summary>
    public UtcTimestamp EndUtc { get; }

    /// <summary>Maximum concurrent task count the resource can support within this window (crew headcount or machine slot count).</summary>
    public int Capacity { get; init; } = 1;

    /// <summary>
    /// Tenant this availability window belongs to (Phase 7 §1.4 Tenant-Aware Scheduling).
    /// Optional/additive -- defaults to <see cref="Guid.Empty"/> ("not populated") for source
    /// compatibility with every existing caller. See <see cref="SchedulableTask.TenantId"/> for
    /// the matching guard this enables.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>Creates a validated ResourceAvailabilityWindow.</summary>
    public ResourceAvailabilityWindow(SchedulingResourceId resourceId, UtcTimestamp startUtc, UtcTimestamp endUtc)
    {
        if (endUtc.Value <= startUtc.Value)
            throw new ArgumentException("ResourceAvailabilityWindow.EndUtc must be after StartUtc.", nameof(endUtc));

        ResourceId = resourceId;
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    /// <summary>Duration spanned by this window.</summary>
    public TimeSpan Duration => EndUtc.Value - StartUtc.Value;

    /// <summary>True when the supplied [start, end) interval falls entirely within this window.</summary>
    public bool Contains(UtcTimestamp start, UtcTimestamp end) =>
        start.Value >= StartUtc.Value && end.Value <= EndUtc.Value;

    /// <summary>True when the supplied [start, end) interval overlaps this window at all.</summary>
    public bool Overlaps(UtcTimestamp start, UtcTimestamp end) =>
        start.Value < EndUtc.Value && end.Value > StartUtc.Value;
}
