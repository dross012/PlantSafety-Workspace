// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side repository interface for Incident timeline entries.
//   Implemented in PlantSafety.Infrastructure (read-optimised stored procedures, Dapper).
//   Backs GetIncidentTimelineQuery (UI Gaps Scoping, IncidentManagement Part 1).
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>Read-side projection of a single <c>IncidentTimelineEntry</c> row.</summary>
public sealed record IncidentTimelineEntryReadModel
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

/// <summary>
/// Read-side repository for Incident timeline entries, persisted to
/// <c>[SafetyEvents].[IncidentTimelineEntry]</c> by <c>IIncidentRepository</c>'s
/// Assign/Escalate/AddNote/Resolve/Archive persistence paths.
/// </summary>
public interface IIncidentTimelineReadRepository
{
    /// <summary>
    /// Returns a page of timeline entries for the given incident, ordered chronologically
    /// (oldest first). The returned tuple contains the page items and the total entry count.
    /// </summary>
    Task<(IReadOnlyList<IncidentTimelineEntryReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid incidentId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
