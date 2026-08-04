// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DriverQualification record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Models;

/// <summary>A single DriverQualification record.</summary>
public sealed record DriverQualificationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DriverQualificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the driver this qualification belongs to.</summary>
    public required Guid DriverUserId { get; init; }

    /// <summary>Classification: CommercialDriverLicense, HazmatEndorsement, ForkliftCertified, DefensiveDrivingCertified.</summary>
    public required string QualificationType { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string QualificationStatus { get; init; }

    /// <summary>License or credential number.</summary>
    public required string LicenseNumber { get; init; }

    /// <summary>UTC timestamp the credential expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
