// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single WasteContainer record.
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.WasteContainers.Models;

/// <summary>A single hazardous waste accumulation container record.</summary>
public sealed record WasteContainerItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid WasteContainerId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the parent AccumulationArea.</summary>
    public required Guid AccumulationAreaId { get; init; }

    /// <summary>Identity of the accumulated WasteStream.</summary>
    public required Guid WasteStreamId { get; init; }

    /// <summary>Physical container label / barcode.</summary>
    public required string ContainerLabel { get; init; }

    /// <summary>Rated capacity in gallons.</summary>
    public required decimal CapacityGallons { get; init; }

    /// <summary>Regulatory accumulation time limit in days.</summary>
    public required int RegulatoryTimeLimitDays { get; init; }

    /// <summary>UTC timestamp the container was opened.</summary>
    public required DateTime OpenedAtUtc { get; init; }

    /// <summary>UTC timestamp the container was closed, if applicable.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }
}
