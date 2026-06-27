// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Certification aggregate root.</summary>
public readonly record struct CertificationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CertificationId wrapping a non-empty GUID.</summary>
    public CertificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CertificationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CertificationId using Uuid7.</summary>
    public static CertificationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CertificationId.</summary>
    public static CertificationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CertificationId.</summary>
    public static CertificationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
