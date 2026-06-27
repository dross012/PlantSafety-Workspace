// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Maintenance
//  Layer:          Domain
//  File:           MaintenanceReference.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Cross-domain read-only reference type for Maintenance. Used via StrongId-only cross-aggregate references.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CrossDomain.Machines.Maintenance;

/// <summary>
/// Read-only cross-domain reference for Maintenance.
/// Other aggregates reference Maintenance by <see cref="MaintenanceId"/> only — never by this type directly.
/// </summary>
public sealed record MaintenanceReference
{
    /// <summary>Strong-typed identity.</summary>
    public MaintenanceId Id { get; init; }

    /// <summary>Display name for UI rendering.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Whether this reference is currently active.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Strong-typed Uuid7-backed identity for cross-domain Maintenance references.</summary>
public readonly record struct MaintenanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a MaintenanceId wrapping a non-empty GUID.</summary>
    public MaintenanceId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("MaintenanceId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new MaintenanceId using Uuid7.</summary>
    public static MaintenanceId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static MaintenanceId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
