// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AIInvestigation domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AIInvestigation domain events.</summary>
public readonly record struct AIInvestigationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AIInvestigationEventId wrapping a non-empty GUID.</summary>
    public AIInvestigationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AIInvestigationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AIInvestigationEventId using Uuid7.</summary>
    public static AIInvestigationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
