// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           IAutomatedIncidentDetectionFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AutomatedIncidentDetection aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Interfaces;

/// <summary>Factory interface for constructing <see cref="AutomatedIncidentDetection"/> aggregates.</summary>
public interface IAutomatedIncidentDetectionFactory
{
    /// <summary>Creates a new AutomatedIncidentDetection aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}