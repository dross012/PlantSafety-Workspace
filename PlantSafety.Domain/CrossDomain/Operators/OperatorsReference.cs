// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Operators
//  Layer:          Domain
//  File:           OperatorsReference.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-domain read-only reference type for Operators. Used via StrongId-only cross-aggregate references.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CrossDomain.Operators;

/// <summary>
/// Read-only cross-domain reference for Operators.
/// Other aggregates reference Operators by <see cref="OperatorsId"/> only — never by this type directly.
/// </summary>
public sealed record OperatorsReference
{
    /// <summary>Strong-typed identity.</summary>
    public OperatorsId Id { get; init; }

    /// <summary>Display name for UI rendering.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Whether this reference is currently active.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Strong-typed Uuid7-backed identity for cross-domain Operators references.</summary>
public readonly record struct OperatorsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a OperatorsId wrapping a non-empty GUID.</summary>
    public OperatorsId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("OperatorsId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new OperatorsId using Uuid7.</summary>
    public static OperatorsId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static OperatorsId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
