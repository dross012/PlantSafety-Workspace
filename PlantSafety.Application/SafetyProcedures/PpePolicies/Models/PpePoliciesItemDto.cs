// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PpePolicies record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.PpePolicies.Models;

/// <summary>A single PpePolicies record (PPE procedures define when, where, and how protective equipment must be used: issuance, inspection, enforcement in designated zones, and replacement schedules.).</summary>
public sealed record PpePoliciesItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PpePoliciesId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Policy Title.</summary>
    public required string PolicyTitle { get; init; }

    /// <summary>Classification: HeadProtection, EyeFaceProtection, HandProtection, HearingProtection, FootProtection.</summary>
    public required string PpeCategory { get; init; }

    /// <summary>Assigned Zone.</summary>
    public required string AssignedZone { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string PolicyStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}