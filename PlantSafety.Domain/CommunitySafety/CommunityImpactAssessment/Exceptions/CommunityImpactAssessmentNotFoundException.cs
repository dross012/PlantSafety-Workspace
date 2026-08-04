// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityImpactAssessment aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityImpactAssessment"/> aggregate lookup finds no match.</summary>
public sealed class CommunityImpactAssessmentNotFoundException : CommunityImpactAssessmentDomainException
{
    /// <summary>Creates a not-found exception for the given CommunityImpactAssessment identity.</summary>
    public CommunityImpactAssessmentNotFoundException(CommunityImpactAssessmentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CommunityImpactAssessment with id '{id}' was not found.",
            "CommunityImpactAssessment.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
