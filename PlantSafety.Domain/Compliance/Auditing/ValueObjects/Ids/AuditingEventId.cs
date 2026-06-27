// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Auditing domain events.</summary>
public readonly record struct AuditingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AuditingEventId wrapping a non-empty GUID.</summary>
    public AuditingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditingEventId using Uuid7.</summary>
    public static AuditingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
