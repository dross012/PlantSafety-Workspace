// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle transition rules for the CommunityImpactAssessment
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Rules;

/// <summary>Pure lifecycle transition rules for the <see cref="Aggregates.CommunityImpactAssessment"/> aggregate.</summary>
public static class CommunityImpactAssessmentLifecycleRules
{
    /// <summary>Returns whether a transition from <paramref name="from"/> to <paramref name="to"/> is permitted.</summary>
    public static bool CanTransition(CommunityImpactAssessmentStatus from, CommunityImpactAssessmentStatus to) => (from, to) switch
    {
        (CommunityImpactAssessmentStatus.Recorded, CommunityImpactAssessmentStatus.Finalized) => true,
        (CommunityImpactAssessmentStatus.Finalized, CommunityImpactAssessmentStatus.Archived) => true,
        _ => false,
    };
}
