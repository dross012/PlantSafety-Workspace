// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing MachineProcessMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Factories;

/// <summary>Default factory implementation for constructing <see cref="MachineProcessMonitoring"/> aggregates.</summary>
public sealed class MachineProcessMonitoringFactory : IMachineProcessMonitoringFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}