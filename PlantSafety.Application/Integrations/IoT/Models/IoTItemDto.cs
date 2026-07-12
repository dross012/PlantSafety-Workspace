// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO representing an IoT sensor/device telemetry reading or event ingested via integration.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.IoT.Models;

/// <summary>Represents an IoT sensor/device telemetry reading or event ingested via integration.</summary>
public sealed record IoTItemDto
{
    /// <summary>Identifier of the ingested IoT reading/event.</summary>
    public required Guid ReadingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the originating IoT device.</summary>
    public required Guid DeviceId { get; init; }

    /// <summary>Sensor type that produced the reading (e.g., Temperature, GasLevel, Vibration).</summary>
    public required string SensorType { get; init; }

    /// <summary>Numeric reading value reported by the device.</summary>
    public required double ReadingValue { get; init; }

    /// <summary>Current reading status (e.g., Normal/Warning/Alert).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the reading was recorded by the device.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the service principal that ingested the reading.</summary>
    public required Guid IngestedBy { get; init; }
}
