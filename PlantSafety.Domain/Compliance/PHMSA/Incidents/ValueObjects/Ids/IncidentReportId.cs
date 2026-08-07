// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the IncidentReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the IncidentReport aggregate root.</summary>
public readonly record struct IncidentReportId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an IncidentReportId wrapping a non-empty GUID.</summary>
    public IncidentReportId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentReportId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentReportId using Uuid7.</summary>
    public static IncidentReportId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an IncidentReportId.</summary>
    public static IncidentReportId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an IncidentReportId.</summary>
    public static IncidentReportId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
