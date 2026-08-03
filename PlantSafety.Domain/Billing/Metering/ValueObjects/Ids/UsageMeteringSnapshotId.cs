// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           UsageMeteringSnapshotId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for a UsageMeteringSnapshot. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Billing.Metering.ValueObjects.Ids;

/// <summary>Strong-typed identity for a <see cref="Aggregates.UsageMeteringSnapshot"/>.</summary>
public readonly record struct UsageMeteringSnapshotId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a UsageMeteringSnapshotId wrapping a non-empty GUID.</summary>
    public UsageMeteringSnapshotId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UsageMeteringSnapshotId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new UsageMeteringSnapshotId using Uuid7.</summary>
    public static UsageMeteringSnapshotId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a UsageMeteringSnapshotId.</summary>
    public static UsageMeteringSnapshotId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
