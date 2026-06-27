// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for a planning project.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for planning projects.</summary>
public readonly record struct ProjectId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a ProjectId wrapping a non-empty GUID.</summary>
    public ProjectId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("ProjectId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new ProjectId using Uuid7.</summary>
    public static ProjectId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static ProjectId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
