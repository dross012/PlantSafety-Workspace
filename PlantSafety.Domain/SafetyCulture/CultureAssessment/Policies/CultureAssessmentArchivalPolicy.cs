// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for CultureAssessment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Policies;

/// <summary>Determines whether a <see cref="CultureAssessment"/> aggregate is eligible for archival.</summary>
public static class CultureAssessmentArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the CultureAssessment may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment aggregate)
        => aggregate.Status == CultureAssessmentStatus.Closed;
}