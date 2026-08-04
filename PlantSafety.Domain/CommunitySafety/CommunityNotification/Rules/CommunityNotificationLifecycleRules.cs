// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle transition rules for the CommunityNotification
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Rules;

/// <summary>Pure lifecycle transition rules for the <see cref="Aggregates.CommunityNotification"/> aggregate.</summary>
public static class CommunityNotificationLifecycleRules
{
    /// <summary>Returns whether a transition from <paramref name="from"/> to <paramref name="to"/> is permitted.</summary>
    public static bool CanTransition(CommunityNotificationStatus from, CommunityNotificationStatus to) => (from, to) switch
    {
        (CommunityNotificationStatus.Sent, CommunityNotificationStatus.Acknowledged) => true,
        (CommunityNotificationStatus.Sent, CommunityNotificationStatus.Failed) => true,
        (CommunityNotificationStatus.Sent, CommunityNotificationStatus.Archived) => true,
        (CommunityNotificationStatus.Acknowledged, CommunityNotificationStatus.Archived) => true,
        (CommunityNotificationStatus.Failed, CommunityNotificationStatus.Archived) => true,
        _ => false,
    };
}
