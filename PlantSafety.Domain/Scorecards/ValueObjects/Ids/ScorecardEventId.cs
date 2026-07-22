// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Scorecard domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scorecards.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Scorecard domain events.</summary>
public readonly record struct ScorecardEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ScorecardEventId wrapping a non-empty GUID.</summary>
    public ScorecardEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ScorecardEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ScorecardEventId using Uuid7.</summary>
    public static ScorecardEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}