// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunityImpactAssessment
//                  aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunityImpactAssessment aggregate root.</summary>
public readonly record struct CommunityImpactAssessmentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityImpactAssessmentId wrapping a non-empty GUID.</summary>
    public CommunityImpactAssessmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityImpactAssessmentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityImpactAssessmentId using Uuid7.</summary>
    public static CommunityImpactAssessmentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityImpactAssessmentId.</summary>
    public static CommunityImpactAssessmentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunityImpactAssessmentId.</summary>
    public static CommunityImpactAssessmentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
