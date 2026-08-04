// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the AutonomousVehicleSafety module, mirroring the
//   PlantSafety.Api read-model shapes for AutonomousVehicleAsset, OperatingGeofence,
//   SafetyCaseValidation, and HumanRobotInteractionEvent. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.AutonomousVehicleSafety.Models;

/// <summary>A single autonomous vehicle/mobile-robot asset (AGV/AMR/autonomous yard truck).</summary>
public sealed class AutonomousVehicleAssetDto
{
    /// <summary>Gets the asset record identifier.</summary>
    public Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Gets the human-readable asset tag/title.</summary>
    public string AssetTag { get; init; } = string.Empty;

    /// <summary>Gets the classification.</summary>
    public string VehicleType { get; init; } = string.Empty;

    /// <summary>Gets the rated degree of autonomy.</summary>
    public string AutonomyLevel { get; init; } = string.Empty;

    /// <summary>Gets the current operating mode.</summary>
    public string CurrentAutonomyMode { get; init; } = string.Empty;

    /// <summary>Gets the cumulative count of recorded autonomy-mode transitions.</summary>
    public int ModeTransitionCount { get; init; }

    /// <summary>Gets the overall lifecycle status classification.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; init; }
}

/// <summary>A single operating-domain geofence (exclusion/restricted/operating/speed-limit zone) with violations.</summary>
public sealed class OperatingGeofenceDto
{
    /// <summary>Gets the geofence record identifier.</summary>
    public Guid OperatingGeofenceId { get; init; }

    /// <summary>Gets the geofence name.</summary>
    public string GeofenceName { get; init; } = string.Empty;

    /// <summary>Gets the zone classification.</summary>
    public string ZoneType { get; init; } = string.Empty;

    /// <summary>Gets the cumulative count of violations detected against this geofence.</summary>
    public int ViolationCount { get; init; }

    /// <summary>Gets the overall lifecycle status classification.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the most recent audit, if any.</summary>
    public DateTime? LastAuditedAtUtc { get; init; }
}

/// <summary>Response payload for the active-geofence-violations read-model query.</summary>
public sealed class OperatingGeofenceResponseDto
{
    /// <summary>Gets the matching geofence violation records. Never null; empty when no results.</summary>
    public IReadOnlyList<OperatingGeofenceDto> Items { get; init; } = [];
}

/// <summary>A single safety-case validation record (ANSI/RIA R15.08 / ISO 3691-4 evidence trail).</summary>
public sealed class SafetyCaseValidationDto
{
    /// <summary>Gets the safety-case validation record identifier.</summary>
    public Guid SafetyCaseValidationId { get; init; }

    /// <summary>Gets the identity of the autonomous vehicle asset this safety case applies to.</summary>
    public Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Gets the standard the safety case is validated against.</summary>
    public string StandardReference { get; init; } = string.Empty;

    /// <summary>Gets the version identifier of the safety-case document.</summary>
    public string DocumentVersion { get; init; } = string.Empty;

    /// <summary>Gets the overall lifecycle status classification.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the case was validated, if validated.</summary>
    public DateTime? ValidatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the validation expires, if validated.</summary>
    public DateTime? ExpiresAtUtc { get; init; }
}

/// <summary>A single human-robot interaction event (proximity alert, near miss, contact, or e-stop trigger).</summary>
public sealed class HumanRobotInteractionEventDto
{
    /// <summary>Gets the interaction event record identifier.</summary>
    public Guid HumanRobotInteractionEventId { get; init; }

    /// <summary>Gets the identity of the autonomous vehicle asset involved.</summary>
    public Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Gets the interaction classification.</summary>
    public string InteractionType { get; init; } = string.Empty;

    /// <summary>Gets the severity classification.</summary>
    public string SeverityLevel { get; init; } = string.Empty;

    /// <summary>Gets the measured separation distance in meters, if available.</summary>
    public decimal? DistanceMeters { get; init; }

    /// <summary>Gets the overall lifecycle status classification.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the interaction was detected.</summary>
    public DateTime DetectedAtUtc { get; init; }
}

/// <summary>Response payload for the human-robot-interaction-events read-model query.</summary>
public sealed class HumanRobotInteractionEventResponseDto
{
    /// <summary>Gets the matching interaction event records. Never null; empty when no results.</summary>
    public IReadOnlyList<HumanRobotInteractionEventDto> Items { get; init; } = [];
}
