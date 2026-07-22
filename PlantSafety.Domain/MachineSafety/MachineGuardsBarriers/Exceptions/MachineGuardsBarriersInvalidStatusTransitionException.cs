// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineGuardsBarriers lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Exceptions;

/// <summary>Thrown when a <see cref="MachineGuardsBarriers"/> lifecycle transition is not permitted.</summary>
public sealed class MachineGuardsBarriersInvalidStatusTransitionException : MachineGuardsBarriersDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public MachineGuardsBarriersInvalidStatusTransitionException(
        MachineGuardsBarriersId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'MachineGuardsBarriers' {id} from '{from}' to '{to}'.",
            "MachineGuardsBarriers.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}