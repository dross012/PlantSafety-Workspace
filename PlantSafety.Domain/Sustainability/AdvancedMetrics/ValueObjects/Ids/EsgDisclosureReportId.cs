// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EsgDisclosureReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EsgDisclosureReport aggregate root.</summary>
public readonly record struct EsgDisclosureReportId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EsgDisclosureReportId wrapping a non-empty GUID.</summary>
    public EsgDisclosureReportId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EsgDisclosureReportId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EsgDisclosureReportId using Uuid7.</summary>
    public static EsgDisclosureReportId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an EsgDisclosureReportId.</summary>
    public static EsgDisclosureReportId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an EsgDisclosureReportId.</summary>
    public static EsgDisclosureReportId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
