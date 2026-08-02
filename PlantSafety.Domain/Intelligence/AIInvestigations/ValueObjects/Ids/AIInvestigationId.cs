// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AIInvestigation aggregate root.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at all
//                  -- only a placeholder marker class (AIInvestigationsModuleMarker) existed.
//                  Mirrors PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids
//                  .RootCauseAnalysisId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;

/// <summary>Strong-typed identity for the AIInvestigation aggregate root.</summary>
public readonly record struct AIInvestigationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AIInvestigationId wrapping a non-empty GUID.</summary>
    public AIInvestigationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AIInvestigationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AIInvestigationId using Uuid7.</summary>
    public static AIInvestigationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AIInvestigationId.</summary>
    public static AIInvestigationId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
