// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineEntry.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a single normalized entry within a
//                  unified AuditTimeline: one of an event, a state-change, a concurrency
//                  conflict, or a scheduling decision, ordered by UtcTimestamp. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Enums;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

/// <summary>A single normalized entry within a reconstructed audit timeline.</summary>
public sealed record AuditTimelineEntry
{
    /// <summary>Strong-typed identity of this entry.</summary>
    public AuditTimelineEntryId AuditTimelineEntryId { get; init; }

    /// <summary>Kind of history this entry represents.</summary>
    public required AuditTimelineEntryType EntryType { get; init; }

    /// <summary>Type discriminator of the entity/aggregate this entry belongs to.</summary>
    public required string EntityType { get; init; }

    /// <summary>Identity of the entity/aggregate this entry belongs to.</summary>
    public Guid EntityId { get; init; }

    /// <summary>Short, human-readable summary of the entry, safe for printed reports.</summary>
    public required string Summary { get; init; }

    /// <summary>Optional structured detail payload (e.g. serialized before/after state, event body).</summary>
    public string? DetailJson { get; init; }

    /// <summary>Identity of the actor associated with this entry, if known.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>UTC instant this entry occurred. The sole ordering key for timeline reconstruction.</summary>
    public UtcTimestamp UtcTimestamp { get; init; }

    /// <summary>Correlation identifier linking this entry to the originating operation, if known.</summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>Creates a new AuditTimelineEntry.</summary>
    public static AuditTimelineEntry Create(
        AuditTimelineEntryType entryType, string entityType, Guid entityId, string summary,
        UtcTimestamp utcTimestamp, string? detailJson = null, Guid? actorId = null, Guid? correlationId = null)
        => new()
        {
            AuditTimelineEntryId = AuditTimelineEntryId.New(),
            EntryType = entryType,
            EntityType = (entityType ?? string.Empty).Trim(),
            EntityId = entityId,
            Summary = (summary ?? string.Empty).Trim(),
            DetailJson = detailJson,
            ActorId = actorId,
            UtcTimestamp = utcTimestamp,
            CorrelationId = correlationId,
        };
}
