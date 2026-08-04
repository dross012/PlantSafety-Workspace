// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityImpactAssessment lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityImpactAssessment"/> lifecycle transition is not permitted.</summary>
public sealed class CommunityImpactAssessmentInvalidStatusTransitionException : CommunityImpactAssessmentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CommunityImpactAssessmentInvalidStatusTransitionException(
        CommunityImpactAssessmentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CommunityImpactAssessment '{id}' from '{from}' to '{to}'.",
            "CommunityImpactAssessment.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
