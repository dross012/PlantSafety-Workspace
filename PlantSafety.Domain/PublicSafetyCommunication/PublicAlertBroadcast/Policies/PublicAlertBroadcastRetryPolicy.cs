// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastRetryPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing whether a failed PublicAlertBroadcast is eligible for
//                  automatic retry. Feeds the broadcast delivery success rate KPI.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Policies;

/// <summary>Determines whether a failed <see cref="PublicAlertBroadcast"/> is eligible for automatic retry.</summary>
public static class PublicAlertBroadcastRetryPolicy
{
    /// <summary>Maximum number of automatic retry attempts for a failed broadcast.</summary>
    public const int MaxRetryAttempts = 3;

    /// <summary>Returns <see langword="true"/> when the broadcast is eligible for retry.</summary>
    public static bool IsEligibleForRetry(PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast aggregate, int attemptsSoFar)
        => aggregate.Status == PublicAlertBroadcastStatus.Failed && attemptsSoFar < MaxRetryAttempts;
}
