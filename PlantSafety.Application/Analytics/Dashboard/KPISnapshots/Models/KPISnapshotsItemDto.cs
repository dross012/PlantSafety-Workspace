// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single KPI snapshot.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Models;

/// <summary>A single point-in-time KPI snapshot.</summary>
public sealed record KPISnapshotsItemDto
{
    /// <summary>Snapshot identifier.</summary>
    public required Guid SnapshotId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the KPI captured (e.g., Near Miss Rate, Audit Completion %).</summary>
    public required string KpiName { get; init; }

    /// <summary>Value of the KPI at the time of the snapshot.</summary>
    public required decimal KpiValue { get; init; }

    /// <summary>Unit of measure for the KPI value (e.g., Percent, Count, Rate).</summary>
    public required string UnitOfMeasure { get; init; }

    /// <summary>Current status of the snapshot (e.g., Provisional, Final).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the snapshot was captured.</summary>
    public required DateTime SnapshotAtUtc { get; init; }
}
