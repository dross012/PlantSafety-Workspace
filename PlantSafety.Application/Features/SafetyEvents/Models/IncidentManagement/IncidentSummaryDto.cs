// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Lightweight summary DTO for list and search query responses.
//   Omits heavy description fields; suitable for paginated lists.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;

/// <summary>
/// Lightweight summary projection of an Incident, used in paginated list and search results.
/// </summary>
public sealed record IncidentSummaryDto
{
    /// <summary>Unique identifier of the incident (UUIDv7).</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Tenant owning this incident.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Short descriptive title of the incident.</summary>
    public required string Title { get; init; }

    /// <summary>UTC timestamp when the incident occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>Current workflow status.</summary>
    public required string Status { get; init; }

    /// <summary>Physical or logical location.</summary>
    public required string Location { get; init; }

    /// <summary>Worker who reported the incident.</summary>
    public required Guid ReportedByWorkerId { get; init; }
}
