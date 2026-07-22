// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing CultureAssessment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Factories;

/// <summary>Default factory implementation for constructing <see cref="CultureAssessment"/> aggregates.</summary>
public sealed class CultureAssessmentFactory : ICultureAssessmentFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}