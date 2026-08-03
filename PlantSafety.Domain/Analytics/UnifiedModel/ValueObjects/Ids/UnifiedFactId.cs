// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           UnifiedFactId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed surrogate key shared by all fact-table rows in the
//                  unified analytics data model. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed surrogate key for a unified analytics fact-table row.</summary>
public readonly record struct UnifiedFactId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a UnifiedFactId wrapping a non-empty GUID.</summary>
    public UnifiedFactId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UnifiedFactId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new UnifiedFactId using Uuid7.</summary>
    public static UnifiedFactId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a UnifiedFactId.</summary>
    public static UnifiedFactId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
