// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunityConcern record.
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Models;

/// <summary>A single CommunityConcern record (neighbor complaint/concern intake).</summary>
public sealed record CommunityConcernItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunityConcernId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Classification: Noise, Odor, Dust, Traffic, IncidentImpact, EnvironmentalRelease, GeneralSafety, Other.</summary>
    public required string Category { get; init; }

    /// <summary>Free-text severity (Low, Medium, High, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Facility area the concern relates to.</summary>
    public required string FacilityArea { get; init; }

    /// <summary>Narrative description of the concern.</summary>
    public required string Description { get; init; }

    /// <summary>Weak reference to the reporting CommunityContact, if any.</summary>
    public Guid? ReportedByContactId { get; init; }

    /// <summary>Weak reference to the related SafetyEvents incident, if any.</summary>
    public Guid? RelatedIncidentId { get; init; }

    /// <summary>Overall lifecycle status.</summary>
    public required string ConcernStatus { get; init; }

    /// <summary>UTC instant the concern was logged.</summary>
    public required DateTime LoggedAtUtc { get; init; }

    /// <summary>UTC instant the concern was resolved, if resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
