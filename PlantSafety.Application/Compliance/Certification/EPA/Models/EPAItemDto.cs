// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single EPA certification record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Certification.EPA.Models;

/// <summary>A single EPA environmental certification/permit record.</summary>
public sealed record EPAItemDto
{
    /// <summary>Certification record identifier.</summary>
    public required Guid CertificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the certified permit holder.</summary>
    public required string PermitHolderName { get; init; }

    /// <summary>EPA regulatory program (e.g., RCRA, CAA, CWA).</summary>
    public required string EpaProgram { get; init; }

    /// <summary>Current certification status (e.g., Active/Expired/Revoked).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the certification was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the certification expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who verified the certification.</summary>
    public required Guid VerifiedBy { get; init; }
}
