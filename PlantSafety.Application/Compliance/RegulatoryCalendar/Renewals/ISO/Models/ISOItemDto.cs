// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single ISO record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Models;

/// <summary>A single ISO read-model item.</summary>
public sealed record ISOItemDto
{
    /// <summary>Renewal Id identifier.</summary>
    public required Guid RenewalId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Certification Standard.</summary>
    public required string CertificationStandard { get; init; }

    /// <summary>Expires At Utc.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Responsible Party.</summary>
    public required Guid ResponsibleParty { get; init; }
}
