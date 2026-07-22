// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for RoutineSafetyAuditsIncidentReporting domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for RoutineSafetyAuditsIncidentReporting domain events.</summary>
public readonly record struct RoutineSafetyAuditsIncidentReportingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RoutineSafetyAuditsIncidentReportingEventId wrapping a non-empty GUID.</summary>
    public RoutineSafetyAuditsIncidentReportingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoutineSafetyAuditsIncidentReportingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RoutineSafetyAuditsIncidentReportingEventId using Uuid7.</summary>
    public static RoutineSafetyAuditsIncidentReportingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}