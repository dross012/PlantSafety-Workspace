// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for a governed AI/ML model registry entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.AiGovernance.Models;

/// <summary>
/// Read-side projection of a governed AI/ML model registry entry, per the Intelligence.ML
/// Manual's model/feature/training/inference/drift governance rules (ML1-4/D1-3).
/// </summary>
public sealed record GovernedModelDto
{
    /// <summary>Unique identifier of the governed model (UUIDv7).</summary>
    public required Guid ModelId { get; init; }

    /// <summary>Tenant owning this model registration.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this model is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Model display name.</summary>
    public required string ModelName { get; init; }

    /// <summary>Model version identifier.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Governance approval status (e.g., PendingReview, Approved, Rejected, Deprecated).</summary>
    public required string GovernanceStatus { get; init; }

    /// <summary>Assessed risk tier (e.g., Low, Medium, High, Critical).</summary>
    public required string RiskTier { get; init; }

    /// <summary>Current model drift status (e.g., Stable, Drifting, Critical).</summary>
    public required string DriftStatus { get; init; }

    /// <summary>UTC timestamp of the most recent governance review.</summary>
    public required DateTime LastReviewedAtUtc { get; init; }

    /// <summary>Display name of the reviewer who last assessed this model.</summary>
    public string? ReviewerName { get; init; }
}
