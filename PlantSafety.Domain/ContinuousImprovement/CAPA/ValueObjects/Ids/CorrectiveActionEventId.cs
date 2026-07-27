// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionEventId.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CAPA CorrectiveAction domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CAPA CorrectiveAction domain events.</summary>
public readonly record struct CorrectiveActionEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CorrectiveActionEventId wrapping a non-empty GUID.</summary>
    public CorrectiveActionEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CorrectiveActionEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CorrectiveActionEventId using Uuid7.</summary>
    public static CorrectiveActionEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
