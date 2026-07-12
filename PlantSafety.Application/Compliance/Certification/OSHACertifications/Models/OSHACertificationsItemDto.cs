// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single OSHA certification record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Certification.OSHACertifications.Models;

/// <summary>A single OSHA certification record.</summary>
public sealed record OSHACertificationsItemDto
{
    /// <summary>Certification record identifier.</summary>
    public required Guid CertificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the certified individual.</summary>
    public required string HolderName { get; init; }

    /// <summary>OSHA standard the certification covers (e.g., OSHA 10-Hour, OSHA 30-Hour).</summary>
    public required string OshaStandard { get; init; }

    /// <summary>Current certification status (e.g., Active/Expired/Revoked).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the certification was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the certification expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who verified the certification.</summary>
    public required Guid VerifiedBy { get; init; }
}
