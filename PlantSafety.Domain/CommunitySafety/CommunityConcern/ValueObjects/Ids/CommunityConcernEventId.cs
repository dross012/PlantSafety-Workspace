// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunityConcern domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a CommunityConcern domain event.</summary>
public readonly record struct CommunityConcernEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunityConcernEventId wrapping a non-empty GUID.</summary>
    public CommunityConcernEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunityConcernEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunityConcernEventId using Uuid7.</summary>
    public static CommunityConcernEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunityConcernEventId.</summary>
    public static CommunityConcernEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
