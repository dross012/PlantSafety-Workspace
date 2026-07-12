// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single SOPs record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Procedures.SOPs.Models;

/// <summary>A single SOPs read-model item.</summary>
public sealed record SOPsItemDto
{
    /// <summary>SOP Id identifier.</summary>
    public required Guid SOPId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Process Area.</summary>
    public required string ProcessArea { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
