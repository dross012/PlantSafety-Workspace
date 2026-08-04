// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single SafetyCaseValidation record.
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;

/// <summary>A single SafetyCaseValidation record (ANSI/RIA R15.08 / ISO 3691-4 evidence trail).</summary>
public sealed record SafetyCaseValidationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid SafetyCaseValidationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the autonomous vehicle asset this safety case applies to.</summary>
    public required Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Standard the safety case is validated against.</summary>
    public required string StandardReference { get; init; }

    /// <summary>Version identifier of the safety-case document.</summary>
    public required string DocumentVersion { get; init; }

    /// <summary>Overall lifecycle status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the case was validated, if validated.</summary>
    public DateTime? ValidatedAtUtc { get; init; }

    /// <summary>UTC timestamp the validation expires, if validated.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
