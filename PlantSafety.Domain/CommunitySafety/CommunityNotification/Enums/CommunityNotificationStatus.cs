// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CommunityNotification aggregate root (Outreach &amp;
//                  Notification History subdomain). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.CommunityNotification"/> aggregate root.</summary>
public enum CommunityNotificationStatus
{
    /// <summary>Notification has been sent. Initial state on Send.</summary>
    Sent = 0,

    /// <summary>Recipient acknowledged receipt (feeds the notification acknowledgement-rate KPI).</summary>
    Acknowledged = 1,

    /// <summary>Delivery failed (e.g. bounced email, invalid number).</summary>
    Failed = 2,

    /// <summary>Notification has been archived for long-term retention. Terminal state.</summary>
    Archived = 3,
}
