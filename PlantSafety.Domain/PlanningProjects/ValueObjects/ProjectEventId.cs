// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Project domain events. Phase 4 full
//                  build-out: mirrors PlantSafety.Domain.SafetyEvents.RootCauseAnalysis
//                  .ValueObjects.Ids.RootCauseAnalysisEventId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for Project domain events.</summary>
public readonly record struct ProjectEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProjectEventId wrapping a non-empty GUID.</summary>
    public ProjectEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProjectEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProjectEventId using Uuid7.</summary>
    public static ProjectEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
