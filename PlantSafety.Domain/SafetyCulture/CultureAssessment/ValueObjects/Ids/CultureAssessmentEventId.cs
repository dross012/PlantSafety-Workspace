// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CultureAssessment domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CultureAssessment domain events.</summary>
public readonly record struct CultureAssessmentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CultureAssessmentEventId wrapping a non-empty GUID.</summary>
    public CultureAssessmentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CultureAssessmentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CultureAssessmentEventId using Uuid7.</summary>
    public static CultureAssessmentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}