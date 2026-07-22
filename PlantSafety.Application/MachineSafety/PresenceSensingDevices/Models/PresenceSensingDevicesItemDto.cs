// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PresenceSensingDevices record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.PresenceSensingDevices.Models;

/// <summary>A single PresenceSensingDevices record (Presence-sensing devices automatically stop machinery when a worker enters a danger zone: light curtains, safety laser scanners, and pressure-sensitive mats for robotic cells and automated lines.).</summary>
public sealed record PresenceSensingDevicesItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PresenceSensingDevicesId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Device Title.</summary>
    public required string DeviceTitle { get; init; }

    /// <summary>Classification: LightCurtain, SafetyLaserScanner, PressureSensitiveMat.</summary>
    public required string DeviceType { get; init; }

    /// <summary>Equipment Location.</summary>
    public required string EquipmentLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string DeviceStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}