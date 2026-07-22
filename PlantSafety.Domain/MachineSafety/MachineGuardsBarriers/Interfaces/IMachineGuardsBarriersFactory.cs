// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           IMachineGuardsBarriersFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing MachineGuardsBarriers aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces;

/// <summary>Factory interface for constructing <see cref="MachineGuardsBarriers"/> aggregates.</summary>
public interface IMachineGuardsBarriersFactory
{
    /// <summary>Creates a new MachineGuardsBarriers aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}