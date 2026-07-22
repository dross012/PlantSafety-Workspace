// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AuditEngagement domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AuditEngagement domain events.</summary>
public readonly record struct AuditEngagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AuditEngagementEventId wrapping a non-empty GUID.</summary>
    public AuditEngagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditEngagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditEngagementEventId using Uuid7.</summary>
    public static AuditEngagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}