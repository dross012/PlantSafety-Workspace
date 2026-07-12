// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.CompetencyManagement
//  Layer:          Domain
//  File:           CompetencyManagementId.cs
//  Created:        2026-07-12
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.CompetencyManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CompetencyManagement aggregate root.</summary>
public readonly record struct CompetencyManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CompetencyManagementId wrapping a non-empty GUID.</summary>
    public CompetencyManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CompetencyManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CompetencyManagementId using Uuid7.</summary>
    public static CompetencyManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CompetencyManagementId.</summary>
    public static CompetencyManagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CompetencyManagementId.</summary>
    public static CompetencyManagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
