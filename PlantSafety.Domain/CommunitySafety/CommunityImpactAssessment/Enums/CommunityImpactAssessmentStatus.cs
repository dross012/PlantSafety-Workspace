// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CommunityImpactAssessment aggregate root (Community
//                  Impact Assessment subdomain). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.CommunityImpactAssessment"/> aggregate root.</summary>
public enum CommunityImpactAssessmentStatus
{
    /// <summary>Assessment has been recorded and may still be revised. Initial state on Record.</summary>
    Recorded = 0,

    /// <summary>Assessment has been finalized and is no longer revisable.</summary>
    Finalized = 1,

    /// <summary>Assessment has been archived (soft-deleted) for long-term retention. Terminal state.</summary>
    Archived = 2,
}
