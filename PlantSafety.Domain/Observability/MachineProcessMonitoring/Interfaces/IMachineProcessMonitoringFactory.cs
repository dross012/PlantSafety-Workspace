// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           IMachineProcessMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing MachineProcessMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces;

/// <summary>Factory interface for constructing <see cref="MachineProcessMonitoring"/> aggregates.</summary>
public interface IMachineProcessMonitoringFactory
{
    /// <summary>Creates a new MachineProcessMonitoring aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}