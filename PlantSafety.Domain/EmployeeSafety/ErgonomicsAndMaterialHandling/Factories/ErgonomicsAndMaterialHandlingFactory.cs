// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ErgonomicsAndMaterialHandling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Factories;

/// <summary>Default factory implementation for constructing <see cref="ErgonomicsAndMaterialHandling"/> aggregates.</summary>
public sealed class ErgonomicsAndMaterialHandlingFactory : IErgonomicsAndMaterialHandlingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}