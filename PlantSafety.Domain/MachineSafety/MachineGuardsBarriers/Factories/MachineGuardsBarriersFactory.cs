// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing MachineGuardsBarriers aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Factories;

/// <summary>Default factory implementation for constructing <see cref="MachineGuardsBarriers"/> aggregates.</summary>
public sealed class MachineGuardsBarriersFactory : IMachineGuardsBarriersFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}