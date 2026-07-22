// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MachineGuardingInspections lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Exceptions;

/// <summary>Thrown when a <see cref="MachineGuardingInspections"/> lifecycle transition is not permitted.</summary>
public sealed class MachineGuardingInspectionsInvalidStatusTransitionException : MachineGuardingInspectionsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public MachineGuardingInspectionsInvalidStatusTransitionException(
        MachineGuardingInspectionsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'MachineGuardingInspections' {id} from '{from}' to '{to}'.",
            "MachineGuardingInspections.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}