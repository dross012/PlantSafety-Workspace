// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernCategory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure classification of the subject matter of a community concern/complaint.
//                  No behavior/metadata beyond the label itself, so this is a simple enum per the
//                  Enums Rule (2026 Revision). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;

/// <summary>Classification of the subject matter of a <see cref="Aggregates.CommunityConcern"/>.</summary>
public enum CommunityConcernCategory
{
    /// <summary>Noise complaint (equipment, traffic, alarms).</summary>
    Noise = 0,

    /// <summary>Odor complaint.</summary>
    Odor = 1,

    /// <summary>Dust or particulate complaint.</summary>
    Dust = 2,

    /// <summary>Traffic or road-safety concern related to plant operations.</summary>
    Traffic = 3,

    /// <summary>Concern about a specific on-site incident affecting the community.</summary>
    IncidentImpact = 4,

    /// <summary>Concern about environmental release or water/soil impact.</summary>
    EnvironmentalRelease = 5,

    /// <summary>General safety concern not otherwise categorized.</summary>
    GeneralSafety = 6,

    /// <summary>Any other concern not covered by the above categories.</summary>
    Other = 7,
}
