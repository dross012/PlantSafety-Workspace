// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Certification domain events.</summary>
public readonly record struct CertificationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CertificationEventId wrapping a non-empty GUID.</summary>
    public CertificationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CertificationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CertificationEventId using Uuid7.</summary>
    public static CertificationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
