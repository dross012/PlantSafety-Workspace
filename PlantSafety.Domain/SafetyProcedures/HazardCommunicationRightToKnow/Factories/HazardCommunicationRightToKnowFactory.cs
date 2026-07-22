// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazardCommunicationRightToKnow aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Factories;

/// <summary>Default factory implementation for constructing <see cref="HazardCommunicationRightToKnow"/> aggregates.</summary>
public sealed class HazardCommunicationRightToKnowFactory : IHazardCommunicationRightToKnowFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}