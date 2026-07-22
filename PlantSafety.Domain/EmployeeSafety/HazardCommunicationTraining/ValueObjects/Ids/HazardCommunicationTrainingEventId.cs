// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for HazardCommunicationTraining domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazardCommunicationTraining domain events.</summary>
public readonly record struct HazardCommunicationTrainingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardCommunicationTrainingEventId wrapping a non-empty GUID.</summary>
    public HazardCommunicationTrainingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardCommunicationTrainingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardCommunicationTrainingEventId using Uuid7.</summary>
    public static HazardCommunicationTrainingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}