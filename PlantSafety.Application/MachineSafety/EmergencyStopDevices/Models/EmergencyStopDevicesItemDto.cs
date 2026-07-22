// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EmergencyStopDevices record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.EmergencyStopDevices.Models;

/// <summary>A single EmergencyStopDevices record (Emergency Stop (E-Stop) devices allow workers to immediately halt machine motion during unsafe conditions via push-button stops, pull-cord systems, and cable-actuated stops.).</summary>
public sealed record EmergencyStopDevicesItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EmergencyStopDevicesId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Device Title.</summary>
    public required string DeviceTitle { get; init; }

    /// <summary>Classification: PushButtonStop, PullCordSystem, CableActuatedStop.</summary>
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