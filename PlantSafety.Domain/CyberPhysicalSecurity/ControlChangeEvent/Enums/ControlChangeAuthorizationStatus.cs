// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeAuthorizationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Authorization classification of a recorded OT control-system change.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;

/// <summary>Authorization classification of a recorded OT control-system change.</summary>
public enum ControlChangeAuthorizationStatus
{
    /// <summary>Change matches an approved change-management record.</summary>
    Authorized = 0,

    /// <summary>Change is awaiting authorization review.</summary>
    PendingReview = 1,

    /// <summary>Change has no matching approval and is treated as unauthorized.</summary>
    Unauthorized = 2
}
