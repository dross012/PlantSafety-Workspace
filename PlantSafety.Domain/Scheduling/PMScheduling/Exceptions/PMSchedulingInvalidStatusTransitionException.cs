// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PMScheduling lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Exceptions;

/// <summary>Thrown when a <see cref="PMScheduling"/> lifecycle transition is not permitted.</summary>
public sealed class PMSchedulingInvalidStatusTransitionException : PMSchedulingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PMSchedulingInvalidStatusTransitionException(
        PMSchedulingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'PMScheduling' {id} from '{from}' to '{to}'.",
            "PMScheduling.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}