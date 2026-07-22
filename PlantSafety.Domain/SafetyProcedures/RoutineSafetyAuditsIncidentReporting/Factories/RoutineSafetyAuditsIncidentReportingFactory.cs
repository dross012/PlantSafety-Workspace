// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing RoutineSafetyAuditsIncidentReporting aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Factories;

/// <summary>Default factory implementation for constructing <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregates.</summary>
public sealed class RoutineSafetyAuditsIncidentReportingFactory : IRoutineSafetyAuditsIncidentReportingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}