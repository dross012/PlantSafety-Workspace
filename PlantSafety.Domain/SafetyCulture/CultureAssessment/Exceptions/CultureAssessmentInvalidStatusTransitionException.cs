// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CultureAssessment lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Exceptions;

/// <summary>Thrown when a <see cref="CultureAssessment"/> lifecycle transition is not permitted.</summary>
public sealed class CultureAssessmentInvalidStatusTransitionException : CultureAssessmentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CultureAssessmentInvalidStatusTransitionException(
        CultureAssessmentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'CultureAssessment' " + id + " from '" + from + "' to '" + to + "'.",
            "CultureAssessment.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}