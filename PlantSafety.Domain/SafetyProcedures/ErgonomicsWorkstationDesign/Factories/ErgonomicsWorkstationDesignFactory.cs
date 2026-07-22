// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ErgonomicsWorkstationDesign aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Factories;

/// <summary>Default factory implementation for constructing <see cref="ErgonomicsWorkstationDesign"/> aggregates.</summary>
public sealed class ErgonomicsWorkstationDesignFactory : IErgonomicsWorkstationDesignFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}