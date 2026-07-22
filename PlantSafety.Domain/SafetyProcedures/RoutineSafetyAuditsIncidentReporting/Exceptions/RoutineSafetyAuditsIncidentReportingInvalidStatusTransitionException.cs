// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RoutineSafetyAuditsIncidentReporting lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Exceptions;

/// <summary>Thrown when a <see cref="RoutineSafetyAuditsIncidentReporting"/> lifecycle transition is not permitted.</summary>
public sealed class RoutineSafetyAuditsIncidentReportingInvalidStatusTransitionException : RoutineSafetyAuditsIncidentReportingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RoutineSafetyAuditsIncidentReportingInvalidStatusTransitionException(
        RoutineSafetyAuditsIncidentReportingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'RoutineSafetyAuditsIncidentReporting' {id} from '{from}' to '{to}'.",
            "RoutineSafetyAuditsIncidentReporting.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}