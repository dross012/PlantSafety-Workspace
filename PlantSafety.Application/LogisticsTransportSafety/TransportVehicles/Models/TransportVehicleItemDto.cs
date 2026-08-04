// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single TransportVehicle record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Models;

/// <summary>A single TransportVehicle record (on-site material/personnel transport vehicle).</summary>
public sealed record TransportVehicleItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid TransportVehicleId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Vehicle title.</summary>
    public required string VehicleTitle { get; init; }

    /// <summary>Classification: Truck, Forklift, Trailer, Van, HeavyEquipmentCarrier.</summary>
    public required string VehicleType { get; init; }

    /// <summary>Plate number or asset tag.</summary>
    public required string VehicleIdentifier { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string VehicleStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
