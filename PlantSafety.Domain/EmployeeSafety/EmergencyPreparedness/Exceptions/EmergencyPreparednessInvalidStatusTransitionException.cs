// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyPreparedness lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyPreparedness"/> lifecycle transition is not permitted.</summary>
public sealed class EmergencyPreparednessInvalidStatusTransitionException : EmergencyPreparednessDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EmergencyPreparednessInvalidStatusTransitionException(
        EmergencyPreparednessId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'EmergencyPreparedness' {id} from '{from}' to '{to}'.",
            "EmergencyPreparedness.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}