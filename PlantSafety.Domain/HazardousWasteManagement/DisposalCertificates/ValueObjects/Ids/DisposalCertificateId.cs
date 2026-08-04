// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the DisposalCertificate aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DisposalCertificate aggregate root.</summary>
public readonly record struct DisposalCertificateId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DisposalCertificateId wrapping a non-empty GUID.</summary>
    public DisposalCertificateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DisposalCertificateId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DisposalCertificateId using Uuid7.</summary>
    public static DisposalCertificateId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DisposalCertificateId.</summary>
    public static DisposalCertificateId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DisposalCertificateId.</summary>
    public static DisposalCertificateId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
