// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a Credential's status (badge/credential PII is access-
//   restricted; only the fields required to answer status/lifecycle questions are exposed here).
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.Credentials.Models;

/// <summary>Status projection for a single Credential record.</summary>
public sealed record CredentialStatusDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CredentialId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Masked badge number (last 4 digits only, PII-restricted).</summary>
    public required string BadgeNumberMasked { get; init; }

    /// <summary>Classification of the holder.</summary>
    public required string HolderType { get; init; }

    /// <summary>Access-level label granted by this credential.</summary>
    public required string AccessLevel { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the credential was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the credential expires, if any.</summary>
    public DateTime? ExpiresAtUtc { get; init; }
}
