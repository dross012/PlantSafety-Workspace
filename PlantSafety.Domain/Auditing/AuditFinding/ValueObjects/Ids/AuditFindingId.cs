// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AuditFinding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AuditFinding aggregate root.</summary>
public readonly record struct AuditFindingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AuditFindingId wrapping a non-empty GUID.</summary>
    public AuditFindingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditFindingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditFindingId using Uuid7.</summary>
    public static AuditFindingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AuditFindingId.</summary>
    public static AuditFindingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AuditFindingId.</summary>
    public static AuditFindingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}