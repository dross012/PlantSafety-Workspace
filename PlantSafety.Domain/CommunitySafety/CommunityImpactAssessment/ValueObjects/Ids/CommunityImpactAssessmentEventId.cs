// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunityImpactAssessment domain
//                  events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CommunityImpactAssessment domain event.</summary>
public readonly record struct CommunityImpactAssessmentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityImpactAssessmentEventId wrapping a non-empty GUID.</summary>
    public CommunityImpactAssessmentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityImpactAssessmentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityImpactAssessmentEventId using Uuid7.</summary>
    public static CommunityImpactAssessmentEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityImpactAssessmentEventId.</summary>
    public static CommunityImpactAssessmentEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
