// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           ISlowlyChangingDimension.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Contract for dimension tables that track attribute history using the
//                  Kimball Slowly Changing Dimension Type 2 pattern (effective-dated rows with
//                  a current-row flag). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;

/// <summary>
/// Contract implemented by dimension-table read-model projections that use the SCD Type 2
/// pattern: every attribute change inserts a new row with a new surrogate key, closes out the
/// prior row's <see cref="EffectiveToUtc"/>, and exactly one row per natural/business key has
/// <see cref="IsCurrent"/> equal to <see langword="true"/> at any point in time.
/// </summary>
public interface ISlowlyChangingDimension
{
    /// <summary>Surrogate key of this dimension row version. Unique per row, changes every SCD2 revision.</summary>
    Guid SurrogateKey { get; }

    /// <summary>Stable natural/business key that identifies the real-world entity across all its row versions.</summary>
    Guid BusinessKey { get; }

    /// <summary>UTC instant from which this row version is/was effective (inclusive).</summary>
    UtcTimestamp EffectiveFromUtc { get; }

    /// <summary>UTC instant this row version stopped being effective (exclusive), or null while still current.</summary>
    UtcTimestamp? EffectiveToUtc { get; }

    /// <summary>True when this row is the current, active version of the business key.</summary>
    bool IsCurrent { get; }

    /// <summary>Monotonically increasing revision number for this business key, starting at 1.</summary>
    int RevisionNumber { get; }
}
