// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           IHazardCommunicationTrainingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HazardCommunicationTraining aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces;

/// <summary>Factory interface for constructing <see cref="HazardCommunicationTraining"/> aggregates.</summary>
public interface IHazardCommunicationTrainingFactory
{
    /// <summary>Creates a new HazardCommunicationTraining aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}