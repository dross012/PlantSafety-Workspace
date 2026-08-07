// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PackagingSpecification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PackagingSpecification aggregate root.</summary>
public readonly record struct PackagingSpecificationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PackagingSpecificationId wrapping a non-empty GUID.</summary>
    public PackagingSpecificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PackagingSpecificationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PackagingSpecificationId using Uuid7.</summary>
    public static PackagingSpecificationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PackagingSpecificationId.</summary>
    public static PackagingSpecificationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PackagingSpecificationId.</summary>
    public static PackagingSpecificationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
