// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.WorkCenters
//  Layer:          Domain
//  File:           WorkCentersReference.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-domain read-only reference type for WorkCenters. Used via StrongId-only cross-aggregate references.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CrossDomain.WorkCenters;

/// <summary>
/// Read-only cross-domain reference for WorkCenters.
/// Other aggregates reference WorkCenters by <see cref="WorkCentersId"/> only — never by this type directly.
/// </summary>
public sealed record WorkCentersReference
{
    /// <summary>Strong-typed identity.</summary>
    public WorkCentersId Id { get; init; }

    /// <summary>Display name for UI rendering.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Whether this reference is currently active.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Strong-typed Uuid7-backed identity for cross-domain WorkCenters references.</summary>
public readonly record struct WorkCentersId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a WorkCentersId wrapping a non-empty GUID.</summary>
    public WorkCentersId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("WorkCentersId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new WorkCentersId using Uuid7.</summary>
    public static WorkCentersId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static WorkCentersId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
