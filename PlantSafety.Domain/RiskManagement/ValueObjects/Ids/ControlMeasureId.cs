// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           ControlMeasureId.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a ControlMeasure entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a ControlMeasure entity.</summary>
public readonly record struct ControlMeasureId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ControlMeasureId wrapping a non-empty GUID.</summary>
    public ControlMeasureId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ControlMeasureId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ControlMeasureId using Uuid7.</summary>
    public static ControlMeasureId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ControlMeasureId.</summary>
    public static ControlMeasureId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
