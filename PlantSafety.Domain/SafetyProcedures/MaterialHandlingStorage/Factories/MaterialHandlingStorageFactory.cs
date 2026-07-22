// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing MaterialHandlingStorage aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Factories;

/// <summary>Default factory implementation for constructing <see cref="MaterialHandlingStorage"/> aggregates.</summary>
public sealed class MaterialHandlingStorageFactory : IMaterialHandlingStorageFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}