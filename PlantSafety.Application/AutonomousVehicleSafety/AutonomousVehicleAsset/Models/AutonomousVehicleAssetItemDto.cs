// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single AutonomousVehicleAsset record.
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;

/// <summary>A single AutonomousVehicleAsset record (AGV/AMR/autonomous yard truck fleet registry entry).</summary>
public sealed record AutonomousVehicleAssetItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable asset tag/title.</summary>
    public required string AssetTag { get; init; }

    /// <summary>Classification: Agv, Amr, AutonomousYardTruck.</summary>
    public required string VehicleType { get; init; }

    /// <summary>Rated degree of autonomy.</summary>
    public required string AutonomyLevel { get; init; }

    /// <summary>Current operating mode: Manual, Teleoperated, Autonomous.</summary>
    public required string CurrentAutonomyMode { get; init; }

    /// <summary>Cumulative count of recorded autonomy-mode transitions.</summary>
    public required int ModeTransitionCount { get; init; }

    /// <summary>Overall lifecycle status classification.</summary>
    public required string Status { get; init; }

    /// <summary>Reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
