// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single compliance tracking item.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Tracking.Models;

/// <summary>A single compliance tracking item.</summary>
public sealed record TrackingItemDto
{
    /// <summary>Tracking item identifier.</summary>
    public required Guid TrackingItemId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the tracked item.</summary>
    public required string ItemDescription { get; init; }

    /// <summary>Category of the tracked item (e.g., Permit Renewal, Audit Action, Training).</summary>
    public required string Category { get; init; }

    /// <summary>Current tracking status (e.g., OnTrack/AtRisk/Overdue/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the tracking item was last updated.</summary>
    public required DateTime LastUpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the tracked item.</summary>
    public required Guid OwnerId { get; init; }
}
