// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single regulatory requirement record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RegulatoryRequirements.Models;

/// <summary>A single applicable regulatory requirement.</summary>
public sealed record RegulatoryRequirementsItemDto
{
    /// <summary>Requirement identifier.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the requirement.</summary>
    public required string RequirementTitle { get; init; }

    /// <summary>Jurisdiction the requirement is enforced under (e.g., Federal, State, Local).</summary>
    public required string Jurisdiction { get; init; }

    /// <summary>Current requirement status (e.g., Applicable/Superseded/UnderReview).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the requirement became effective.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the user who owns tracking of the requirement.</summary>
    public required Guid OwnerId { get; init; }
}
