// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single LotoSystems record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.LotoSystems.Models;

/// <summary>A single LotoSystems record (LOTO systems isolate hazardous energy sources during maintenance, cleaning, or repair via padlocks/hasps/tags, labeled isolation points, and zero-energy verification procedures.).</summary>
public sealed record LotoSystemsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid LotoSystemsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>System Title.</summary>
    public required string SystemTitle { get; init; }

    /// <summary>Classification: Electrical, Hydraulic, Pneumatic, Thermal, Chemical.</summary>
    public required string EnergySourceType { get; init; }

    /// <summary>Isolation Point Description.</summary>
    public required string IsolationPointDescription { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string SystemStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}