// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single PublicSafetyAgency directory record. Backs
// the Agency Notification Status Board dashboard.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;

/// <summary>A single PublicSafetyAgency directory read-model record.</summary>
public sealed record PublicSafetyAgencyItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PublicSafetyAgencyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current operational status (Active/Inactive).</summary>
    public required string Status { get; init; }

    /// <summary>Legal/display name of the agency.</summary>
    public required string AgencyName { get; init; }

    /// <summary>Classification of the agency.</summary>
    public required string AgencyType { get; init; }

    /// <summary>Description of the jurisdiction/coverage area.</summary>
    public required string JurisdictionDescription { get; init; }

    /// <summary>Primary phone number.</summary>
    public required string PrimaryPhoneNumber { get; init; }

    /// <summary>Primary email address, if any.</summary>
    public string? PrimaryEmail { get; init; }

    /// <summary>Current verification/currency status of the agency's contact data.</summary>
    public required string VerificationStatus { get; init; }

    /// <summary>UTC timestamp of the most recent successful verification, if any.</summary>
    public DateTime? LastVerifiedAtUtc { get; init; }

    /// <summary>UTC timestamp the next scheduled verification is due.</summary>
    public DateTime? NextVerificationDueAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field.
    /// </summary>
    public string? ETag { get; init; }
}
