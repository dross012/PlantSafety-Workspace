// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPETrainingFit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPETrainingFit aggregate root.</summary>
public readonly record struct PPETrainingFitId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPETrainingFitId wrapping a non-empty GUID.</summary>
    public PPETrainingFitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPETrainingFitId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPETrainingFitId using Uuid7.</summary>
    public static PPETrainingFitId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPETrainingFitId.</summary>
    public static PPETrainingFitId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPETrainingFitId.</summary>
    public static PPETrainingFitId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}