// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CrisisManagement domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CrisisManagement domain events.</summary>
public readonly record struct CrisisManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CrisisManagementEventId wrapping a non-empty GUID.</summary>
    public CrisisManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CrisisManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CrisisManagementEventId using Uuid7.</summary>
    public static CrisisManagementEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CrisisManagementEventId.</summary>
    public static CrisisManagementEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
