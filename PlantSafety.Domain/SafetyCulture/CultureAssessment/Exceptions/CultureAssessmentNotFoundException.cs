// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CultureAssessment aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Exceptions;

/// <summary>Thrown when a <see cref="CultureAssessment"/> aggregate lookup finds no match.</summary>
public sealed class CultureAssessmentNotFoundException : CultureAssessmentDomainException
{
    /// <summary>Creates a not-found exception for the given CultureAssessment identity.</summary>
    public CultureAssessmentNotFoundException(CultureAssessmentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "CultureAssessment with id '" + id + "' was not found.",
            "CultureAssessment.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}