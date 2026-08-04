// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Triage lifecycle states for the AccessAnomaly aggregate root (a system-
//                  detected security condition such as tailgating, forced entry, after-hours
//                  access, or a restricted-zone violation).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums;

/// <summary>Triage lifecycle states for the <see cref="AccessAnomaly"/> aggregate root.</summary>
public enum AccessAnomalyStatus
{
    /// <summary>Anomaly has been detected but not yet triaged.</summary>
    Detected = 0,

    /// <summary>Anomaly has been acknowledged by a responder.</summary>
    Acknowledged = 1,

    /// <summary>Anomaly has been escalated (e.g. via AlertEscalationPolicy) for further response.</summary>
    Escalated = 2,

    /// <summary>Anomaly has been fully investigated and resolved.</summary>
    Resolved = 3,
}
