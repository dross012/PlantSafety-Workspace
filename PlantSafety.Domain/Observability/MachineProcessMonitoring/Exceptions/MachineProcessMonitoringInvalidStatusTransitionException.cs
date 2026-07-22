// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineProcessMonitoring lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="MachineProcessMonitoring"/> lifecycle transition is not permitted.</summary>
public sealed class MachineProcessMonitoringInvalidStatusTransitionException : MachineProcessMonitoringDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public MachineProcessMonitoringInvalidStatusTransitionException(
        MachineProcessMonitoringId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'MachineProcessMonitoring' {id} from '{from}' to '{to}'.",
            "MachineProcessMonitoring.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}