// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HazardCommunicationTraining aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazardCommunicationTraining aggregate root.</summary>
public readonly record struct HazardCommunicationTrainingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardCommunicationTrainingId wrapping a non-empty GUID.</summary>
    public HazardCommunicationTrainingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardCommunicationTrainingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardCommunicationTrainingId using Uuid7.</summary>
    public static HazardCommunicationTrainingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazardCommunicationTrainingId.</summary>
    public static HazardCommunicationTrainingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazardCommunicationTrainingId.</summary>
    public static HazardCommunicationTrainingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}