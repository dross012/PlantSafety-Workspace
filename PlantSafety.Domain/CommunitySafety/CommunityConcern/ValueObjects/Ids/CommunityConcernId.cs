// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunityConcern aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunityConcern aggregate root.</summary>
public readonly record struct CommunityConcernId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityConcernId wrapping a non-empty GUID.</summary>
    public CommunityConcernId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityConcernId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityConcernId using Uuid7.</summary>
    public static CommunityConcernId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityConcernId.</summary>
    public static CommunityConcernId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunityConcernId.</summary>
    public static CommunityConcernId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
