// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineSnapshot.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable point-in-time capture of a fully-composed AuditTimeline for a
//                  single entity, used as the "state change history" building block and as the
//                  snapshot-baseline source feeding AuditReport generation (§6.2). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Snapshots;

/// <summary>Immutable point-in-time capture of a fully-composed AuditTimeline for one entity.</summary>
public sealed record AuditTimelineSnapshot
{
    /// <summary>Type discriminator of the entity this timeline belongs to.</summary>
    public required string EntityType { get; init; }

    /// <summary>Identity of the entity this timeline belongs to.</summary>
    public Guid EntityId { get; init; }

    /// <summary>All composed entries, strictly ordered by UtcTimestamp.</summary>
    public required IReadOnlyList<AuditTimelineEntry> Entries { get; init; }

    /// <summary>UTC instant this snapshot was captured/composed.</summary>
    public UtcTimestamp SnapshotAtUtc { get; init; }
}
