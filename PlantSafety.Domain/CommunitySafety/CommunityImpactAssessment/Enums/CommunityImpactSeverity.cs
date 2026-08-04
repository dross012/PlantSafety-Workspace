// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure classification of how severely an on-site incident impacted the
//                  surrounding community. No behavior/metadata beyond the label itself, so this
//                  is a simple enum per the Enums Rule (2026 Revision). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;

/// <summary>Classification of community impact severity for a <see cref="Aggregates.CommunityImpactAssessment"/>.</summary>
public enum CommunityImpactSeverity
{
    /// <summary>No discernible community impact.</summary>
    None = 0,

    /// <summary>Minor, transient community impact (e.g. brief noise/odor).</summary>
    Minor = 1,

    /// <summary>Moderate community impact requiring outreach/notification.</summary>
    Moderate = 2,

    /// <summary>Severe community impact requiring notification and regulatory reporting (e.g. EPCRA).</summary>
    Severe = 3,
}
