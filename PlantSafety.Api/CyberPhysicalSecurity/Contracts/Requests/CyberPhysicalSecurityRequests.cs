// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the CyberPhysicalSecurity module's commands. Domain fields
//   only; envelope fields (CommandId/CorrelationId/TenantId/ActorId/etc.) are populated
//   server-side by RequestEnvelopeFactory.
// SchemaVersion: 1

namespace PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/ot-assets</c>.</summary>
public sealed class RegisterOtAssetRequest
{
    /// <summary>Site this asset is physically located at.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Unique human-readable asset tag/label used on the plant floor.</summary>
    public required string AssetTag { get; init; }

    /// <summary>Descriptive name of the asset.</summary>
    public required string AssetName { get; init; }

    /// <summary>Classification of the OT/ICS asset (Plc, Hmi, Scada, Rtu, Historian, NetworkDevice, Sensor, Actuator, Other).</summary>
    public required string AssetType { get; init; }

    /// <summary>Vendor/manufacturer name.</summary>
    public string? Vendor { get; init; }

    /// <summary>Model/part number.</summary>
    public string? Model { get; init; }

    /// <summary>Currently installed firmware/software version.</summary>
    public string? FirmwareVersion { get; init; }

    /// <summary>OT network zone / Purdue Model level the asset resides in.</summary>
    public string? NetworkZone { get; init; }

    /// <summary>Safety/production criticality rating (Low, Medium, High, Critical).</summary>
    public string? Criticality { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/control-change-events</c>.</summary>
public sealed class RecordControlChangeEventRequest
{
    /// <summary>Site the asset is located at.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Identity of the OT asset the change was observed on.</summary>
    public required Guid OtAssetId { get; init; }

    /// <summary>Human-readable description of the change.</summary>
    public string? ChangeDescription { get; init; }

    /// <summary>Name of the control-system parameter that changed.</summary>
    public required string ChangedParameter { get; init; }

    /// <summary>Prior value of the parameter, as text.</summary>
    public string? OldValue { get; init; }

    /// <summary>New value of the parameter, as text.</summary>
    public string? NewValue { get; init; }

    /// <summary>UTC timestamp the change was observed on the OT network (telemetry timestamp).</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>Identity of the user who initiated the change, if attributable.</summary>
    public Guid? ChangeSourceUserId { get; init; }

    /// <summary>Whether an approved change-management record exists for this change.</summary>
    public bool HasApprovedChangeRecord { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/sensor-integrity-checks</c>.</summary>
public sealed class RunSensorIntegrityCheckRequest
{
    /// <summary>Site the sensor is located at.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Identity of the sensor OT asset checked.</summary>
    public required Guid OtAssetId { get; init; }

    /// <summary>Category of integrity verification performed (Calibration, Signal, Firmware, Tamper).</summary>
    public required string CheckType { get; init; }

    /// <summary>Outcome of the check (Pass, Fail, Inconclusive).</summary>
    public required string Result { get; init; }

    /// <summary>Free-form details of the check outcome.</summary>
    public string? Details { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/sabotage-indicators</c>.</summary>
public sealed class RaiseSabotageIndicatorRequest
{
    /// <summary>Site the indicator relates to.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Origin subsystem that produced this indicator (ControlChange, SensorIntegrity, PhysicalAccess, ExternalIntel, Manual).</summary>
    public required string Source { get; init; }

    /// <summary>Human-readable description of the indicator.</summary>
    public required string Description { get; init; }

    /// <summary>Severity rating (Low, Medium, High, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Identity of the originating record (ControlChangeEventId, SensorIntegrityCheckId, etc.), if applicable.</summary>
    public Guid? SourceReferenceId { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/incidents</c>.</summary>
public sealed class OpenCyberPhysicalIncidentRequest
{
    /// <summary>Site the incident relates to.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short incident title.</summary>
    public required string Title { get; init; }

    /// <summary>Full incident description/narrative.</summary>
    public string? Description { get; init; }

    /// <summary>Severity rating (Low, Medium, High, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Identity of the accountable incident owner/responder.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Identities of the SabotageIndicators that contributed to opening this incident.</summary>
    public IReadOnlyList<Guid>? RelatedSabotageIndicatorIds { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cyberphysicalsecurity/incidents/{incidentId}/close</c>.</summary>
public sealed class CloseCyberPhysicalIncidentRequest
{
}
