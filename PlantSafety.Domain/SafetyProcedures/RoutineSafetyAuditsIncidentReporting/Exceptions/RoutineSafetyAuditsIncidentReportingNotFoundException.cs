// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RoutineSafetyAuditsIncidentReporting aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Exceptions;

/// <summary>Thrown when a <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate lookup finds no match.</summary>
public sealed class RoutineSafetyAuditsIncidentReportingNotFoundException : RoutineSafetyAuditsIncidentReportingDomainException
{
    /// <summary>Creates a not-found exception for the given RoutineSafetyAuditsIncidentReporting identity.</summary>
    public RoutineSafetyAuditsIncidentReportingNotFoundException(RoutineSafetyAuditsIncidentReportingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "RoutineSafetyAuditsIncidentReporting with id '{id}' was not found.",
            "RoutineSafetyAuditsIncidentReporting.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}