// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PreQualificationVetting aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PreQualificationVetting aggregate root.</summary>
public readonly record struct PreQualificationVettingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PreQualificationVettingId wrapping a non-empty GUID.</summary>
    public PreQualificationVettingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PreQualificationVettingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PreQualificationVettingId using Uuid7.</summary>
    public static PreQualificationVettingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PreQualificationVettingId.</summary>
    public static PreQualificationVettingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PreQualificationVettingId.</summary>
    public static PreQualificationVettingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}