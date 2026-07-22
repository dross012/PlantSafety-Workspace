// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CultureAssessment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CultureAssessment aggregate root.</summary>
public readonly record struct CultureAssessmentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CultureAssessmentId wrapping a non-empty GUID.</summary>
    public CultureAssessmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CultureAssessmentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CultureAssessmentId using Uuid7.</summary>
    public static CultureAssessmentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CultureAssessmentId.</summary>
    public static CultureAssessmentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CultureAssessmentId.</summary>
    public static CultureAssessmentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}