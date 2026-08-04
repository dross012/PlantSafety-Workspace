// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Real dispatch lifecycle states for an outbound IncidentNotification sent to an
//                  external public-safety agency.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;

/// <summary>Lifecycle states for the <see cref="IncidentNotification"/> aggregate root.</summary>
public enum IncidentNotificationStatus
{
    /// <summary>Notification created and queued for dispatch.</summary>
    Pending = 0,

    /// <summary>Notification has been dispatched to the agency's channel.</summary>
    Dispatched = 1,

    /// <summary>Agency has acknowledged receipt of the notification.</summary>
    Acknowledged = 2,

    /// <summary>Notification lifecycle is closed (resolved/no further action required).</summary>
    Closed = 3,

    /// <summary>Delivery to the agency's channel failed.</summary>
    Failed = 4,
}
