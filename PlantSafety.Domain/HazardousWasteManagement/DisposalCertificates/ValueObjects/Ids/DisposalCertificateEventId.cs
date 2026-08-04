// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for DisposalCertificate domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DisposalCertificate domain events.</summary>
public readonly record struct DisposalCertificateEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DisposalCertificateEventId wrapping a non-empty GUID.</summary>
    public DisposalCertificateEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DisposalCertificateEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DisposalCertificateEventId using Uuid7.</summary>
    public static DisposalCertificateEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
