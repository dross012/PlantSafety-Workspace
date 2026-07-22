// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           IRoutineSafetyAuditsIncidentReportingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing RoutineSafetyAuditsIncidentReporting aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Interfaces;

/// <summary>Factory interface for constructing <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregates.</summary>
public interface IRoutineSafetyAuditsIncidentReportingFactory
{
    /// <summary>Creates a new RoutineSafetyAuditsIncidentReporting aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}