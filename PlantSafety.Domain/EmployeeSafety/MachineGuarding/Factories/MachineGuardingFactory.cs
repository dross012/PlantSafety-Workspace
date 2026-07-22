// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing MachineGuarding aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Factories;

/// <summary>Default factory implementation for constructing <see cref="MachineGuarding"/> aggregates.</summary>
public sealed class MachineGuardingFactory : IMachineGuardingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding Install(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string guardTitle,
        MachineGuardType guardType,
        string equipmentTag,
        string location,
        bool hasInterlock,
        int inspectionIntervalDays = 90)
        => PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding.Install(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, guardTitle, guardType, equipmentTag, location, hasInterlock, inspectionIntervalDays);
}
