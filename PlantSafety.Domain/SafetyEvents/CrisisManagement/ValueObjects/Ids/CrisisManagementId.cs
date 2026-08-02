// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementId.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CrisisManagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;

/// <summary>Strong-typed identity for the CrisisManagement aggregate root.</summary>
public readonly record struct CrisisManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CrisisManagementId wrapping a non-empty GUID.</summary>
    public CrisisManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CrisisManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CrisisManagementId using Uuid7.</summary>
    public static CrisisManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CrisisManagementId.</summary>
    public static CrisisManagementId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
