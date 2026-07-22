// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single Ppe record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Ppe.Models;

/// <summary>A single Ppe record (Personal Protective Equipment (PPE) serves as the final line of defense against workplace hazards, covering head, eye/face, hand, hearing, foot, and high-visibility protection with fit-testing, inspection, and replacement schedules.).</summary>
public sealed record PpeItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PpeId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Ppe Item Title.</summary>
    public required string PpeItemTitle { get; init; }

    /// <summary>Classification: HeadProtection, EyeFaceProtection, HandProtection, HearingProtection, FootProtection, HighVisibilityClothing.</summary>
    public required string PpeCategory { get; init; }

    /// <summary>Assigned Zone.</summary>
    public required string AssignedZone { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string PpeStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}