// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Auditing aggregate root.</summary>
public readonly record struct AuditingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AuditingId wrapping a non-empty GUID.</summary>
    public AuditingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditingId using Uuid7.</summary>
    public static AuditingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AuditingId.</summary>
    public static AuditingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AuditingId.</summary>
    public static AuditingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
