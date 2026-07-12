// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single ISO 14001 certification record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Certification.ISO14001.Models;

/// <summary>A single ISO 14001 environmental management certification record.</summary>
public sealed record ISO14001ItemDto
{
    /// <summary>Certification record identifier.</summary>
    public required Guid CertificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the accredited certifying body.</summary>
    public required string CertifyingBody { get; init; }

    /// <summary>Scope of the certification (e.g., site, process line).</summary>
    public required string Scope { get; init; }

    /// <summary>Current certification status (e.g., Active/Expired/Suspended).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the certification was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the certification expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who verified the certification.</summary>
    public required Guid VerifiedBy { get; init; }
}
