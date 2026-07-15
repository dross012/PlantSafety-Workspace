// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for a Digital Twin asset registry entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.DigitalTwin.Models;

/// <summary>
/// Read-side projection of a Digital Twin asset. Represents a virtual replica of a
/// physical plant asset (machine, line, or facility) synchronized from telemetry.
/// </summary>
public sealed record DigitalTwinAssetDto
{
    /// <summary>Unique identifier of the digital twin (UUIDv7).</summary>
    public required Guid TwinId { get; init; }

    /// <summary>Tenant owning this twin.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this twin is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the physical asset this twin represents.</summary>
    public required string AssetName { get; init; }

    /// <summary>Asset type/category (e.g., Machine, Line, Facility).</summary>
    public required string AssetType { get; init; }

    /// <summary>Current twin synchronization status (e.g., Synced, Degraded, Offline).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the twin's last successful telemetry sync.</summary>
    public required DateTime LastSyncAtUtc { get; init; }

    /// <summary>Telemetry health score (0.0-1.0) for this twin.</summary>
    public required double TelemetryHealthScore { get; init; }
}
