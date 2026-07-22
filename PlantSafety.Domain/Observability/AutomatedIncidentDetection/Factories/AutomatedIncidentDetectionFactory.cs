// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AutomatedIncidentDetection aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Factories;

/// <summary>Default factory implementation for constructing <see cref="AutomatedIncidentDetection"/> aggregates.</summary>
public sealed class AutomatedIncidentDetectionFactory : IAutomatedIncidentDetectionFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}