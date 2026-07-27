// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionId.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ContinuousImprovement CAPA
//                  CorrectiveAction aggregate root. Wraps the same underlying value persisted as
//                  [ContinuousImprovement].[CorrectiveAction].CaId. Distinct from
//                  PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.CorrectiveActionId,
//                  which identifies the unrelated Accident-child CorrectiveAction entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CAPA CorrectiveAction aggregate root.</summary>
public readonly record struct CorrectiveActionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CorrectiveActionId wrapping a non-empty GUID.</summary>
    public CorrectiveActionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CorrectiveActionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CorrectiveActionId using Uuid7.</summary>
    public static CorrectiveActionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CorrectiveActionId.</summary>
    public static CorrectiveActionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CorrectiveActionId.</summary>
    public static CorrectiveActionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
