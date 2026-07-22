// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPETrainingFit domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPETrainingFit domain events.</summary>
public readonly record struct PPETrainingFitEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPETrainingFitEventId wrapping a non-empty GUID.</summary>
    public PPETrainingFitEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPETrainingFitEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPETrainingFitEventId using Uuid7.</summary>
    public static PPETrainingFitEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}