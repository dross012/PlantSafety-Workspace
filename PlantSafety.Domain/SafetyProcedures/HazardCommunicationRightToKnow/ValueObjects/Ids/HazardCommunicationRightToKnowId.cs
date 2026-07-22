// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HazardCommunicationRightToKnow aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazardCommunicationRightToKnow aggregate root.</summary>
public readonly record struct HazardCommunicationRightToKnowId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardCommunicationRightToKnowId wrapping a non-empty GUID.</summary>
    public HazardCommunicationRightToKnowId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardCommunicationRightToKnowId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardCommunicationRightToKnowId using Uuid7.</summary>
    public static HazardCommunicationRightToKnowId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazardCommunicationRightToKnowId.</summary>
    public static HazardCommunicationRightToKnowId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazardCommunicationRightToKnowId.</summary>
    public static HazardCommunicationRightToKnowId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}