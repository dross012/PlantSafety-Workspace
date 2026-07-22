// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PermitToWork domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PermitToWork domain events.</summary>
public readonly record struct PermitToWorkEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitToWorkEventId wrapping a non-empty GUID.</summary>
    public PermitToWorkEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitToWorkEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitToWorkEventId using Uuid7.</summary>
    public static PermitToWorkEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}