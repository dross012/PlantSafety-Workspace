// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the LocalEmergencyCoordination aggregate root. Mirrors
//                  PlantSafety.Domain.SafetyEvents.EmergencyManagement.Enums.EmergencyManagementStatus.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate root.</summary>
public enum LocalEmergencyCoordinationStatus
{
    /// <summary>Coordination case has been created and awaits activation.</summary>
    Created = 0,

    /// <summary>Coordination case is active: mutual aid, joint plans, and/or liaisons are being managed.</summary>
    Active = 1,

    /// <summary>Coordination case has been completed (all joint response activity concluded).</summary>
    Completed = 2,

    /// <summary>Coordination case has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Coordination case has been archived for long-term/EPCRA compliance retention.</summary>
    Archived = 4,
}
