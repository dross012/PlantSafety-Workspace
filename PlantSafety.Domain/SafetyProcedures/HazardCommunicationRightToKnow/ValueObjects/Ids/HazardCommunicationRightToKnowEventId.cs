// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for HazardCommunicationRightToKnow domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazardCommunicationRightToKnow domain events.</summary>
public readonly record struct HazardCommunicationRightToKnowEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardCommunicationRightToKnowEventId wrapping a non-empty GUID.</summary>
    public HazardCommunicationRightToKnowEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardCommunicationRightToKnowEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardCommunicationRightToKnowEventId using Uuid7.</summary>
    public static HazardCommunicationRightToKnowEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}