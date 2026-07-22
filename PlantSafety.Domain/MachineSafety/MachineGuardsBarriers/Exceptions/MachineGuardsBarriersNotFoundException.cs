// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineGuardsBarriers aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Exceptions;

/// <summary>Thrown when a <see cref="MachineGuardsBarriers"/> aggregate lookup finds no match.</summary>
public sealed class MachineGuardsBarriersNotFoundException : MachineGuardsBarriersDomainException
{
    /// <summary>Creates a not-found exception for the given MachineGuardsBarriers identity.</summary>
    public MachineGuardsBarriersNotFoundException(MachineGuardsBarriersId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "MachineGuardsBarriers with id '{id}' was not found.",
            "MachineGuardsBarriers.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}