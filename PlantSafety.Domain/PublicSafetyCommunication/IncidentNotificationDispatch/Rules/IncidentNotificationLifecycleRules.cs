// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the IncidentNotification aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="IncidentNotification"/> aggregate.</summary>
public static class IncidentNotificationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an acknowledgement may be recorded from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(IncidentNotificationStatus current) => current == IncidentNotificationStatus.Dispatched;

    /// <summary>Returns <see langword="true"/> when the notification may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(IncidentNotificationStatus current) =>
        current is IncidentNotificationStatus.Dispatched or IncidentNotificationStatus.Acknowledged;

    /// <summary>Returns <see langword="true"/> when a delivery failure may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordDeliveryFailure(IncidentNotificationStatus current) =>
        current is IncidentNotificationStatus.Pending or IncidentNotificationStatus.Dispatched;
}
