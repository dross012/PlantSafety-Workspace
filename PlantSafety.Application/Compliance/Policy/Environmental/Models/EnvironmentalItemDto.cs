// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Environmental record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.Environmental.Models;

/// <summary>A single Environmental read-model item.</summary>
public sealed record EnvironmentalItemDto
{
    /// <summary>Policy Id identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Environmental Aspect.</summary>
    public required string EnvironmentalAspect { get; init; }

    /// <summary>Regulatory Reference.</summary>
    public required string RegulatoryReference { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
