// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple lifecycle enum for the Alert aggregate root. Pure classification with
//                  no metadata beyond ordering — satisfies the Simple Enum branch of the Enums
//                  Rule (2.7). Grounded in "14. Alert Logging & Reporting: ... Close-out
//                  verification".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.Alert"/> aggregate root.</summary>
public enum AlertStatus
{
    /// <summary>Alert has been triggered and is awaiting acknowledgement.</summary>
    Triggered = 0,

    /// <summary>Alert has been acknowledged by a responder.</summary>
    Acknowledged = 1,

    /// <summary>Alert response has been dispatched (first responders/maintenance/security notified).</summary>
    Dispatched = 2,

    /// <summary>Alert has been escalated due to lack of timely acknowledgement or response.</summary>
    Escalated = 3,

    /// <summary>Underlying hazard/condition has been resolved.</summary>
    Resolved = 4,

    /// <summary>Alert has been closed with close-out verification recorded.</summary>
    Closed = 5,
}
