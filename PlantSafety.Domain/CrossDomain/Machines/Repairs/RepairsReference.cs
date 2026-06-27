// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Repairs
//  Layer:          Domain
//  File:           RepairsReference.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-domain read-only reference type for Repairs. Used via StrongId-only cross-aggregate references.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CrossDomain.Machines.Repairs;

/// <summary>
/// Read-only cross-domain reference for Repairs.
/// Other aggregates reference Repairs by <see cref="RepairsId"/> only — never by this type directly.
/// </summary>
public sealed record RepairsReference
{
    /// <summary>Strong-typed identity.</summary>
    public RepairsId Id { get; init; }

    /// <summary>Display name for UI rendering.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Whether this reference is currently active.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Strong-typed Uuid7-backed identity for cross-domain Repairs references.</summary>
public readonly record struct RepairsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a RepairsId wrapping a non-empty GUID.</summary>
    public RepairsId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("RepairsId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new RepairsId using Uuid7.</summary>
    public static RepairsId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static RepairsId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
