// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CraneHoistSystems record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

/// <summary>A single CraneHoistSystems record (dedicated crane and hoist equipment safety management: asset registry, load-limit enforcement, inspection &amp; certification, operator qualification, and rigging safety).</summary>
public sealed record CraneHoistSystemsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CraneHoistSystemsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Equipment tag for this crane/hoist asset.</summary>
    public required string AssetTag { get; init; }

    /// <summary>Asset Title.</summary>
    public required string AssetTitle { get; init; }

    /// <summary>Classification: OverheadBridgeCrane, GantryCrane, JibCrane, MobileCrane, Hoist.</summary>
    public required string AssetType { get; init; }

    /// <summary>Manufacturer-rated maximum load capacity, in kilograms.</summary>
    public required decimal RatedLoadCapacityKg { get; init; }

    /// <summary>Most recently observed lift load, in kilograms.</summary>
    public decimal? LastObservedLoadKg { get; init; }

    /// <summary>Count of recorded overload events over the asset's lifetime.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>UTC due date of the next scheduled inspection, if scheduled.</summary>
    public DateTime? InspectionDueAtUtc { get; init; }

    /// <summary>UTC expiry timestamp of the current operator qualification, if any.</summary>
    public DateTime? OperatorQualificationExpiresAtUtc { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string SystemStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
