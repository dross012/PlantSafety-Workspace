// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side DTO for a single Incident timeline entry.
//   Mirrors PlantSafety.UI.Modules.IncidentManagement.Models.IncidentTimelineEntryDto.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;

/// <summary>Read-side projection of a single Incident timeline entry.</summary>
public sealed record IncidentTimelineEntryDto
{
    /// <summary>Entry identifier.</summary>
    public required Guid EntryId { get; init; }

    /// <summary>UTC timestamp of the entry.</summary>
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Actor who performed/generated the entry.</summary>
    public required string Actor { get; init; }

    /// <summary>Event type (StatusChange, Note, Escalation, Resolution).</summary>
    public required string EventType { get; init; }

    /// <summary>Severity associated with the entry at the time it occurred.</summary>
    public required string Severity { get; init; }

    /// <summary>Human-readable description.</summary>
    public required string Description { get; init; }
}
