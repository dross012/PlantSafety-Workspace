// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazardCommunicationTraining aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Factories;

/// <summary>Default factory implementation for constructing <see cref="HazardCommunicationTraining"/> aggregates.</summary>
public sealed class HazardCommunicationTrainingFactory : IHazardCommunicationTrainingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}