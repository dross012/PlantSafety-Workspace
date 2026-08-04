// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle transition rules for the CommunityMeetingRecord
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Rules;

/// <summary>Pure lifecycle transition rules for the <see cref="Aggregates.CommunityMeetingRecord"/> aggregate.</summary>
public static class CommunityMeetingRecordLifecycleRules
{
    /// <summary>Returns whether a transition from <paramref name="from"/> to <paramref name="to"/> is permitted.</summary>
    public static bool CanTransition(CommunityMeetingRecordStatus from, CommunityMeetingRecordStatus to) => (from, to) switch
    {
        (CommunityMeetingRecordStatus.Recorded, CommunityMeetingRecordStatus.Archived) => true,
        _ => false,
    };
}
