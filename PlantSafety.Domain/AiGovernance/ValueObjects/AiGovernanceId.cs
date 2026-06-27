// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the AiGovernance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the AiGovernance aggregate root.</summary>
public readonly record struct AiGovernanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AiGovernanceId wrapping a non-empty GUID.</summary>
    public AiGovernanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AiGovernanceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AiGovernanceId using Uuid7.</summary>
    public static AiGovernanceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AiGovernanceId.</summary>
    public static AiGovernanceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AiGovernanceId.</summary>
    public static AiGovernanceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
