// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a single normalized AuditTimeline entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditTimeline.Models;

/// <summary>Read-model DTO for a single normalized AuditTimeline entry.</summary>
public sealed record AuditTimelineEntryDto
{
    /// <summary>Entry identity.</summary>
    public required Guid AuditTimelineEntryId { get; init; }

    /// <summary>Entry type code (EventRaised/StateChange/ConcurrencyConflict/SchedulingDecision/ComplianceViolation).</summary>
    public required string EntryType { get; init; }

    /// <summary>Type discriminator of the entity this entry belongs to.</summary>
    public required string EntityType { get; init; }

    /// <summary>Identity of the entity this entry belongs to.</summary>
    public required Guid EntityId { get; init; }

    /// <summary>Short, human-readable summary of the entry.</summary>
    public required string Summary { get; init; }

    /// <summary>Optional structured detail payload (JSON).</summary>
    public string? DetailJson { get; init; }

    /// <summary>Identity of the associated actor, if known.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>UTC instant this entry occurred; the ordering key.</summary>
    public required DateTime UtcTimestamp { get; init; }

    /// <summary>Correlation identifier of the originating operation, if known.</summary>
    public Guid? CorrelationId { get; init; }
}
