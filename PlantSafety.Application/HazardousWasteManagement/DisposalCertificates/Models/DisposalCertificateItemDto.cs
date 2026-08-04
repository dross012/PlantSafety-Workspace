// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DisposalCertificate record.
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Models;

/// <summary>A single hazardous waste disposal certificate record.</summary>
public sealed record DisposalCertificateItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DisposalCertificateId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the closing manifest.</summary>
    public required Guid ManifestId { get; init; }

    /// <summary>Identity of the WasteStream ultimately disposed of.</summary>
    public required Guid WasteStreamId { get; init; }

    /// <summary>Identity of the TSDF facility that issued the certificate.</summary>
    public required Guid TsdfFacilityId { get; init; }

    /// <summary>Method by which the waste was disposed of.</summary>
    public required string DisposalMethod { get; init; }

    /// <summary>UTC date the physical disposal occurred.</summary>
    public required DateTime DisposalDateUtc { get; init; }

    /// <summary>UTC timestamp this certificate was received.</summary>
    public required DateTime CertificateReceivedAtUtc { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }
}
