// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           CorrectiveActionId.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a CorrectiveAction (CAPA) entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CorrectiveAction (CAPA) entity.</summary>
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

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
