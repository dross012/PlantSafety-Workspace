// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single ChemicalHandling record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.ChemicalHandling.Models;

/// <summary>A single ChemicalHandling read-model item.</summary>
public sealed record ChemicalHandlingItemDto
{
    /// <summary>Policy Id identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Chemical Name.</summary>
    public required string ChemicalName { get; init; }

    /// <summary>Hazard Class.</summary>
    public required string HazardClass { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
