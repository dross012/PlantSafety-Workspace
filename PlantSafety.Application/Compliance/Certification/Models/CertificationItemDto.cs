// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CertificationItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Certification record. Backs
//                  GetCertificationByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Certification.Models;

/// <summary>A single Certification read-model record.</summary>
public sealed record CertificationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CertificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Worker who holds this certification.</summary>
    public Guid? HolderWorkerId { get; init; }

    /// <summary>Name of the certification.</summary>
    public required string CertificationName { get; init; }

    /// <summary>Issuing/certifying body.</summary>
    public required string CertificationBody { get; init; }

    /// <summary>Certificate/credential number as issued by the certifying body.</summary>
    public required string CertificationNumber { get; init; }

    /// <summary>UTC timestamp the certification was issued.</summary>
    public DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the certification expires, if it has a validity window.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>Indicates whether this certification requires periodic renewal.</summary>
    public bool RenewalRequired { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
