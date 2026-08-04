// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle transition rules for the CommunityContact aggregate.
//                  Consulted by the aggregate before mutating Status; contains no persistence or
//                  infrastructure concerns per the Canonical Domain Service Rule. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Rules;

/// <summary>Pure lifecycle transition rules for the <see cref="Aggregates.CommunityContact"/> aggregate.</summary>
public static class CommunityContactLifecycleRules
{
    /// <summary>Returns whether a transition from <paramref name="from"/> to <paramref name="to"/> is permitted.</summary>
    public static bool CanTransition(CommunityContactStatus from, CommunityContactStatus to) => (from, to) switch
    {
        (CommunityContactStatus.Active, CommunityContactStatus.Inactive) => true,
        (CommunityContactStatus.Active, CommunityContactStatus.Archived) => true,
        (CommunityContactStatus.Inactive, CommunityContactStatus.Active) => true,
        (CommunityContactStatus.Inactive, CommunityContactStatus.Archived) => true,
        _ => false,
    };
}
