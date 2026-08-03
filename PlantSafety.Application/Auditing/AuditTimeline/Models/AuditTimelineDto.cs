// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a fully composed, chronological AuditTimeline for a
//   single entity/aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditTimeline.Models;

/// <summary>Read-model DTO for a fully composed, chronologically-ordered AuditTimeline.</summary>
public sealed record AuditTimelineDto
{
    /// <summary>Type discriminator of the entity this timeline belongs to.</summary>
    public required string EntityType { get; init; }

    /// <summary>Identity of the entity this timeline belongs to.</summary>
    public required Guid EntityId { get; init; }

    /// <summary>All entries, strictly ordered by UtcTimestamp.</summary>
    public required IReadOnlyList<AuditTimelineEntryDto> Entries { get; init; }

    /// <summary>UTC instant this timeline was composed.</summary>
    public required DateTime ComposedAtUtc { get; init; }
}
