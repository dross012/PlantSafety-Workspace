// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the CultureAssessment aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="CultureAssessment"/> aggregate.</summary>
public static class CultureAssessmentLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(CultureAssessmentStatus current) => current == CultureAssessmentStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(CultureAssessmentStatus current) => current == CultureAssessmentStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(CultureAssessmentStatus current) => current == CultureAssessmentStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(CultureAssessmentStatus current) => current == CultureAssessmentStatus.Closed;
}