// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single EPA record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Models;

/// <summary>A single EPA read-model item.</summary>
public sealed record EPAItemDto
{
    /// <summary>Deadline Id identifier.</summary>
    public required Guid DeadlineId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>EPA Regulation.</summary>
    public required string EPARegulation { get; init; }

    /// <summary>Due At Utc.</summary>
    public required DateTime DueAtUtc { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Responsible Party.</summary>
    public required Guid ResponsibleParty { get; init; }
}
