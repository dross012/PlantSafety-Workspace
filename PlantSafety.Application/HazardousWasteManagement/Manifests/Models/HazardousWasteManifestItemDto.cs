// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single HazardousWasteManifest record.
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Models;

/// <summary>A single Uniform Hazardous Waste Manifest record.</summary>
public sealed record HazardousWasteManifestItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazardousWasteManifestId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>EPA Uniform Hazardous Waste Manifest tracking number.</summary>
    public required string ManifestNumber { get; init; }

    /// <summary>Generator's EPA identification number.</summary>
    public required string GeneratorEpaId { get; init; }

    /// <summary>Identity of the manifested waste stream.</summary>
    public required Guid WasteStreamId { get; init; }

    /// <summary>Identity of the assigned transporter, if recorded.</summary>
    public Guid? TransporterId { get; init; }

    /// <summary>Identity of the destination TSDF facility, if recorded.</summary>
    public Guid? TsdfFacilityId { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the manifest was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>UTC timestamp the transporter signed for custody, if recorded.</summary>
    public DateTime? TransporterSignedAtUtc { get; init; }

    /// <summary>UTC timestamp the TSDF facility signed for receipt, if recorded.</summary>
    public DateTime? TsdfReceivedAtUtc { get; init; }

    /// <summary>UTC timestamp the manifest was closed, if applicable.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary><see langword="true"/> once the transporter has signed; the manifest becomes immutable.</summary>
    public required bool IsImmutable { get; init; }

    /// <summary>Most recent discrepancy reason, if any.</summary>
    public string? DiscrepancyReason { get; init; }

    /// <summary>UTC instant the mandatory 3-year regulatory retention period expires.</summary>
    public required DateTime RetentionExpiresAtUtc { get; init; }
}
