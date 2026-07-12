// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single PPE record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.PPE.Models;

/// <summary>A single PPE read-model item.</summary>
public sealed record PPEItemDto
{
    /// <summary>Policy Id identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>PPE Category.</summary>
    public required string PPECategory { get; init; }

    /// <summary>Required Equipment.</summary>
    public required string RequiredEquipment { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
