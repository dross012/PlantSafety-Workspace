// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           ICultureAssessmentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing CultureAssessment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;

/// <summary>Factory interface for constructing <see cref="CultureAssessment"/> aggregates.</summary>
public interface ICultureAssessmentFactory
{
    /// <summary>Creates a new CultureAssessment aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}