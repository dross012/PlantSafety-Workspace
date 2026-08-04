// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single Carrier record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Carriers.Models;

/// <summary>A single Carrier record (fleet or third-party transport carrier).</summary>
public sealed record CarrierItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CarrierId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Carrier name.</summary>
    public required string CarrierName { get; init; }

    /// <summary>Classification: InHouseFleet, ThirdPartyContractCarrier, OwnerOperator.</summary>
    public required string CarrierType { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string CarrierStatus { get; init; }

    /// <summary>Safety rating score (0-100), if qualified.</summary>
    public decimal? SafetyRatingScore { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
