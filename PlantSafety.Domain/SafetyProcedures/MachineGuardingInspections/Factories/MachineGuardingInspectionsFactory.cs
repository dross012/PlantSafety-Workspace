// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing MachineGuardingInspections aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Factories;

/// <summary>Default factory implementation for constructing <see cref="MachineGuardingInspections"/> aggregates.</summary>
public sealed class MachineGuardingInspectionsFactory : IMachineGuardingInspectionsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}