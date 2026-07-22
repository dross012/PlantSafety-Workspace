// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Scorecard aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scorecards.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Scorecard aggregate root.</summary>
public readonly record struct ScorecardId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ScorecardId wrapping a non-empty GUID.</summary>
    public ScorecardId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ScorecardId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ScorecardId using Uuid7.</summary>
    public static ScorecardId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ScorecardId.</summary>
    public static ScorecardId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ScorecardId.</summary>
    public static ScorecardId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}