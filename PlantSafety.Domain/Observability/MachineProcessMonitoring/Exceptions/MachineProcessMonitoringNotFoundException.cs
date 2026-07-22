// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineProcessMonitoring aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="MachineProcessMonitoring"/> aggregate lookup finds no match.</summary>
public sealed class MachineProcessMonitoringNotFoundException : MachineProcessMonitoringDomainException
{
    /// <summary>Creates a not-found exception for the given MachineProcessMonitoring identity.</summary>
    public MachineProcessMonitoringNotFoundException(MachineProcessMonitoringId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"MachineProcessMonitoring with id '{id}' was not found.",
            "MachineProcessMonitoring.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}