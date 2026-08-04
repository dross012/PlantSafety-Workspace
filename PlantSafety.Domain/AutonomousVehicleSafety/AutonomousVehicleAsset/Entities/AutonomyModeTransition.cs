// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomyModeTransition.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity capturing a single autonomy-mode transition (e.g. Manual to
//                  Autonomous, Autonomous to Teleoperated) recorded against an AutonomousVehicleAsset.
//                  Distinct from AutonomousVehicleAssetHistoryEntry, which tracks coarse lifecycle
//                  status transitions; this entity captures the finer-grained autonomy-mode
//                  transitions required for ANSI/RIA R15.08 and ISO 3691-4 evidence trails.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Entities;

/// <summary>Records a single autonomy-mode transition on an <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate.</summary>
public sealed class AutonomyModeTransition
{
    /// <summary>Unique identity of this transition record.</summary>
    public Guid AutonomyModeTransitionId { get; private set; }

    /// <summary>Identity of the parent AutonomousVehicleAsset aggregate.</summary>
    public AutonomousVehicleAssetId AutonomousVehicleAssetId { get; private set; }

    /// <summary>Autonomy mode before the transition.</summary>
    public AutonomyMode FromMode { get; private set; }

    /// <summary>Autonomy mode after the transition.</summary>
    public AutonomyMode ToMode { get; private set; }

    /// <summary>UTC instant the transition occurred.</summary>
    public UtcTimestamp TransitionedAtUtc { get; private set; }

    /// <summary>Reason or trigger for the transition (operator command, geofence exit, fault, etc.).</summary>
    public string? Reason { get; private set; }

    /// <summary>Identity of the user or system actor who triggered the transition.</summary>
    public UserId TriggeredByUserId { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private AutonomyModeTransition() { }

    /// <summary>Creates a new autonomy-mode transition record.</summary>
    public AutonomyModeTransition(
        Guid autonomyModeTransitionId,
        AutonomousVehicleAssetId autonomousVehicleAssetId,
        AutonomyMode fromMode,
        AutonomyMode toMode,
        UtcTimestamp transitionedAtUtc,
        UserId triggeredByUserId,
        string? reason = null)
    {
        AutonomyModeTransitionId = autonomyModeTransitionId;
        AutonomousVehicleAssetId = autonomousVehicleAssetId;
        FromMode = fromMode;
        ToMode = toMode;
        TransitionedAtUtc = transitionedAtUtc;
        TriggeredByUserId = triggeredByUserId;
        Reason = reason;
    }
}
