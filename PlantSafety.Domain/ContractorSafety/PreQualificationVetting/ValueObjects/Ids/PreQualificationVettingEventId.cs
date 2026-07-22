// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PreQualificationVetting domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PreQualificationVetting domain events.</summary>
public readonly record struct PreQualificationVettingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PreQualificationVettingEventId wrapping a non-empty GUID.</summary>
    public PreQualificationVettingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PreQualificationVettingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PreQualificationVettingEventId using Uuid7.</summary>
    public static PreQualificationVettingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}