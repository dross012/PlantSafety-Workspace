// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Employees
//  Layer:          Domain
//  File:           EmployeesReference.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-domain read-only reference type for Employees. Used via StrongId-only cross-aggregate references.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CrossDomain.Employees;

/// <summary>
/// Read-only cross-domain reference for Employees.
/// Other aggregates reference Employees by <see cref="EmployeesId"/> only — never by this type directly.
/// </summary>
public sealed record EmployeesReference
{
    /// <summary>Strong-typed identity.</summary>
    public EmployeesId Id { get; init; }

    /// <summary>Display name for UI rendering.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Whether this reference is currently active.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Strong-typed Uuid7-backed identity for cross-domain Employees references.</summary>
public readonly record struct EmployeesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a EmployeesId wrapping a non-empty GUID.</summary>
    public EmployeesId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("EmployeesId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new EmployeesId using Uuid7.</summary>
    public static EmployeesId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static EmployeesId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
