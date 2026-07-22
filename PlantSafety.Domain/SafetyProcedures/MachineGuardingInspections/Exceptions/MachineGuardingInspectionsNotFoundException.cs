// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineGuardingInspections aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Exceptions;

/// <summary>Thrown when a <see cref="MachineGuardingInspections"/> aggregate lookup finds no match.</summary>
public sealed class MachineGuardingInspectionsNotFoundException : MachineGuardingInspectionsDomainException
{
    /// <summary>Creates a not-found exception for the given MachineGuardingInspections identity.</summary>
    public MachineGuardingInspectionsNotFoundException(MachineGuardingInspectionsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "MachineGuardingInspections with id '{id}' was not found.",
            "MachineGuardingInspections.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}