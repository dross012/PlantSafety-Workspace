// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PublicAlertBroadcast aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PublicAlertBroadcast"/> aggregate.</summary>
public static class PublicAlertBroadcastLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when delivery may be confirmed from <paramref name="current"/>.</summary>
    public static bool CanConfirmDelivery(PublicAlertBroadcastStatus current) => current == PublicAlertBroadcastStatus.Sent;

    /// <summary>Returns <see langword="true"/> when a failure may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordFailure(PublicAlertBroadcastStatus current) =>
        current is PublicAlertBroadcastStatus.Draft or PublicAlertBroadcastStatus.Sent;

    /// <summary>Returns <see langword="true"/> when the broadcast may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(PublicAlertBroadcastStatus current) => current != PublicAlertBroadcastStatus.Cancelled;
}
