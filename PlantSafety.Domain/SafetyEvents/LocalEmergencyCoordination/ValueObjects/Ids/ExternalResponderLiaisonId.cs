// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ExternalResponderLiaisonId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the ExternalResponderLiaison child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for an <see cref="Entities.ExternalResponderLiaison"/> child entity.</summary>
public readonly record struct ExternalResponderLiaisonId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an ExternalResponderLiaisonId wrapping a non-empty GUID.</summary>
    public ExternalResponderLiaisonId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExternalResponderLiaisonId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ExternalResponderLiaisonId using Uuid7.</summary>
    public static ExternalResponderLiaisonId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an ExternalResponderLiaisonId.</summary>
    public static ExternalResponderLiaisonId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
