// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single SafetyPolicies record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.SafetyPolicies.Models;

/// <summary>A single SafetyPolicies read-model item.</summary>
public sealed record SafetyPoliciesItemDto
{
    /// <summary>Policy Id identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Policy Name.</summary>
    public required string PolicyName { get; init; }

    /// <summary>Category.</summary>
    public required string Category { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
