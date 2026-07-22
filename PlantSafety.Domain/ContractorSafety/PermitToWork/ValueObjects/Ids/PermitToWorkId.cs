// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PermitToWork aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PermitToWork aggregate root.</summary>
public readonly record struct PermitToWorkId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitToWorkId wrapping a non-empty GUID.</summary>
    public PermitToWorkId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitToWorkId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitToWorkId using Uuid7.</summary>
    public static PermitToWorkId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PermitToWorkId.</summary>
    public static PermitToWorkId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PermitToWorkId.</summary>
    public static PermitToWorkId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}