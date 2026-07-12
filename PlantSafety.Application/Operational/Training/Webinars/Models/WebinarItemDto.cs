// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single webinar session.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Training.Webinars.Models;

/// <summary>A single scheduled or completed webinar session.</summary>
public sealed record WebinarItemDto
{
    /// <summary>Webinar identifier.</summary>
    public required Guid WebinarId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Webinar title/topic.</summary>
    public required string Title { get; init; }

    /// <summary>Identity of the assigned host.</summary>
    public required Guid HostId { get; init; }

    /// <summary>Current status (e.g., Scheduled/Live/Completed/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the webinar is/was scheduled.</summary>
    public required DateTime ScheduledAtUtc { get; init; }
}
