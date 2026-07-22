// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           IPPEBehavioralMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPEBehavioralMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPEBehavioralMonitoring"/> aggregates.</summary>
public interface IPPEBehavioralMonitoringFactory
{
    /// <summary>Creates a new PPEBehavioralMonitoring aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}