// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           IMachineGuardingFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing MachineGuarding aggregates via Install().
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Interfaces;

/// <summary>Factory interface for constructing <see cref="MachineGuarding"/> aggregates.</summary>
public interface IMachineGuardingFactory
{
    /// <summary>Records the installation of a new machine guard with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding Install(
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
        int inspectionIntervalDays = 90);
}
