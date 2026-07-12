// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.CompetencyManagement
//  Layer:          Domain
//  File:           CompetencyManagementEventId.cs
//  Created:        2026-07-12
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.CompetencyManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CompetencyManagement domain events.</summary>
public readonly record struct CompetencyManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CompetencyManagementEventId wrapping a non-empty GUID.</summary>
    public CompetencyManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CompetencyManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CompetencyManagementEventId using Uuid7.</summary>
    public static CompetencyManagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
