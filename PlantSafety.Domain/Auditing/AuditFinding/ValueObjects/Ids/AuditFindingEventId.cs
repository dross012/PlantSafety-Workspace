// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AuditFinding domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AuditFinding domain events.</summary>
public readonly record struct AuditFindingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AuditFindingEventId wrapping a non-empty GUID.</summary>
    public AuditFindingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditFindingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditFindingEventId using Uuid7.</summary>
    public static AuditFindingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}