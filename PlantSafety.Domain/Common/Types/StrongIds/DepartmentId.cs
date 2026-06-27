// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DepartmentId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Cross-cutting StrongId for organisational departments. Used across
//      Training, HR, Compliance, and Operational modules.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.StrongIds;

/// <summary>Strong-typed department identity used across organisational modules.</summary>
public readonly record struct DepartmentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DepartmentId wrapping a non-empty GUID.</summary>
    public DepartmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DepartmentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DepartmentId using Uuid7.</summary>
    public static DepartmentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DepartmentId.</summary>
    public static DepartmentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DepartmentId.</summary>
    public static DepartmentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
