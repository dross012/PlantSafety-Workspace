// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Checklists record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Checklists.Models;

/// <summary>A single Checklists read-model record.</summary>
public sealed record ChecklistsItemDto
{
    /// <summary>Checklists record identifier.</summary>
    public required Guid ChecklistId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Checklist name.</summary>
    public required string Name { get; init; }

    /// <summary>Checklist category grouping.</summary>
    public required string Category { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid CompletedBy { get; init; }
}
