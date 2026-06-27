// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligenceId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the AiSafetyIntelligence aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the AiSafetyIntelligence aggregate root.</summary>
public readonly record struct AiSafetyIntelligenceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AiSafetyIntelligenceId wrapping a non-empty GUID.</summary>
    public AiSafetyIntelligenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AiSafetyIntelligenceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AiSafetyIntelligenceId using Uuid7.</summary>
    public static AiSafetyIntelligenceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AiSafetyIntelligenceId.</summary>
    public static AiSafetyIntelligenceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AiSafetyIntelligenceId.</summary>
    public static AiSafetyIntelligenceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
