// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for CultureAssessment aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Snapshots;

/// <summary>Immutable snapshot of a <see cref="CultureAssessment"/> aggregate for fast rehydration.</summary>
public sealed record CultureAssessmentSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CultureAssessmentId CultureAssessmentId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CultureAssessmentStatus Status { get; init; }

    /// <summary>Overall computed culture health score at snapshot time, when available.</summary>
    public decimal? OverallCultureScore { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}