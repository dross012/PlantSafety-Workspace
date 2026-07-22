// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the RoutineSafetyAuditsIncidentReporting aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the RoutineSafetyAuditsIncidentReporting aggregate root.</summary>
public readonly record struct RoutineSafetyAuditsIncidentReportingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RoutineSafetyAuditsIncidentReportingId wrapping a non-empty GUID.</summary>
    public RoutineSafetyAuditsIncidentReportingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoutineSafetyAuditsIncidentReportingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RoutineSafetyAuditsIncidentReportingId using Uuid7.</summary>
    public static RoutineSafetyAuditsIncidentReportingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RoutineSafetyAuditsIncidentReportingId.</summary>
    public static RoutineSafetyAuditsIncidentReportingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RoutineSafetyAuditsIncidentReportingId.</summary>
    public static RoutineSafetyAuditsIncidentReportingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}