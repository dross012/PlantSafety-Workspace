// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Real delivery lifecycle states for a PublicAlertBroadcast sent to public alert
//                  systems (EAS/WEA/IPAWS and similar).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;

/// <summary>Lifecycle states for the <see cref="PublicAlertBroadcast"/> aggregate root.</summary>
public enum PublicAlertBroadcastStatus
{
    /// <summary>Broadcast drafted but not yet sent.</summary>
    Draft = 0,

    /// <summary>Broadcast has been sent to the target public alert system.</summary>
    Sent = 1,

    /// <summary>Delivery to the target system has been confirmed.</summary>
    Delivered = 2,

    /// <summary>Delivery to the target system failed.</summary>
    Failed = 3,

    /// <summary>Broadcast was cancelled before or after sending.</summary>
    Cancelled = 4,
}
